using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

[RequireComponent(typeof(PrivilegeRequester))]
public class MLImageCapture : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static MLImageCapture Instance;

    /// <summary>
    /// Keep counts of the taps for image renaming
    /// </summary>
    private int captureCount = 0;
    
    [System.Serializable]
    private class ImageCaptureEvent : UnityEvent<Texture2D>
    {}

    [SerializeField, Space, Tooltip("ControllerConnectionHandler reference.")]
    private ControllerConnectionHandler _controllerConnectionHandler;


    private bool _isCameraConnected = false;
    private bool _isCapturing = false;
    private bool _hasStarted = false;
    private bool _doPrivPopup = false;
    private bool _hasShownPrivPopup = false;
    private Thread _captureThread = null;

    /// <summary>
    /// The example is using threads on the call to MLCamera.CaptureRawImageAsync to alleviate the blocking
    /// call at the beginning of CaptureRawImageAsync, and the safest way to prevent race conditions here is to
    /// lock our access into the MLCamera class, so that we don't accidentally shut down the camera
    /// while the thread is attempting to work
    /// </summary>
    private object _cameraLockObject = new object();

    private PrivilegeRequester _privilegeRequester;

    /// <summary>
    /// Called on initialization
    /// </summary>
    private void Awake()
    {
        Instance = this;
        
        if(_controllerConnectionHandler == null)
        {
            Debug.LogError("Error: ImageCaptureExample._controllerConnectionHandler is not set, disabling script.");
            enabled = false;
            return;
        }

        // If not listed here, the PrivilegeRequester assumes the request for
        // the privileges needed, CameraCapture in this case, are in the editor.
        _privilegeRequester = GetComponent<PrivilegeRequester>();

        // Before enabling the Camera, the scene must wait until the privilege has been granted.
        _privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;
    }

    /// <summary>
    /// Runs at initialization right after Awake method
    /// </summary>
    void Start()
    {
        // Clean up the LocalState folder of this application from all photos stored
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                Debug.LogFormat("Cannot delete file: ", file.Name);
            }
        }
    }
    
    private void OnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (_controllerConnectionHandler.IsControllerValid(controllerId) && MLInputControllerButton.Bumper == button && !_isCapturing)
        {
            TriggerAsyncCapture();
        }
    }
    
    public void OnImageCaptured(Texture2D texture)
    {
        PotRecognitionManager.Instance.PlaceAnalysisLabel();
        
        // Set the cursor color to red
        PotRecognitionManager.Instance.cursor.GetComponent<Renderer>().material.color = Color.red;

        string filename = string.Format(@"CapturedImage{0}.jpg", captureCount);
        var filePath = Path.Combine(Application.persistentDataPath, filename);   
        
        captureCount++;     
        
        File.WriteAllBytes(filePath, texture.EncodeToJPG());
        // Call the image analysis
        StartCoroutine(PotAnalyzer.Instance.AnalyseLastImageCaptured(filePath)); 
    }
    
    /// <summary>
    /// Stops all capture pending actions
    /// </summary>
    internal void ResetImageCapture()
    {
        // Set the cursor color to green
        PotRecognitionManager.Instance.cursor.GetComponent<Renderer>().material.color = Color.green;
    }
    
    /// <summary>
    /// Stop the camera, unregister callbacks, and stop input and privileges APIs.
    /// </summary>
    void OnDisable()
    {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        lock (_cameraLockObject)
        {
            if (_isCameraConnected)
            {
                MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
                _isCapturing = false;
                DisableMLCamera();
            }
        }
    }

    /// <summary>
    /// Cannot make the assumption that a reality privilege is still granted after
    /// returning from pause. Return the application to the state where it
    /// requests privileges needed and clear out the list of already granted
    /// privileges. Also, disable the camera and unregister callbacks.
    /// </summary>
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            lock (_cameraLockObject)
            {
                if (_isCameraConnected)
                {
                    MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
                    _isCapturing = false;
                    DisableMLCamera();
                }
            }

            MLInput.OnControllerButtonDown -= OnButtonDown;

            _hasStarted = false;
        }
    }

    void OnDestroy()
    {
        if (_privilegeRequester != null)
        {
            _privilegeRequester.OnPrivilegesDone -= HandlePrivilegesDone;
        }
    }

    private void Update()
    {
        if (_doPrivPopup && !_hasShownPrivPopup)
        {
            Instantiate(Resources.Load("PrivilegeDeniedError"));
            _doPrivPopup = false;
            _hasShownPrivPopup = true;
        }
    }
    
    /// <summary>
    /// Captures a still image using the device's camera and returns
    /// the data path where it is saved.
    /// </summary>
    /// <param name="fileName">The name of the file to be saved to.</param>
    public void TriggerAsyncCapture()
    {
        if (_captureThread == null || (!_captureThread.IsAlive))
        {
            ThreadStart captureThreadStart = new ThreadStart(CaptureThreadWorker);
            _captureThread = new Thread(captureThreadStart);
            _captureThread.Start();
        }
        else
        {
            Debug.Log("Previous thread has not finished, unable to begin a new capture just yet.");
        }
    }
    
    private void EnableMLCamera()
    {
        lock (_cameraLockObject)
        {
            MLResult result = MLCamera.Start();
            if (result.IsOk)
            {
                result = MLCamera.Connect();
                _isCameraConnected = true;
            }
            else
            {
                if (result.Code == MLResultCode.PrivilegeDenied)
                {
                    Instantiate(Resources.Load("PrivilegeDeniedError"));
                }

                Debug.LogErrorFormat("Error: ImageCaptureExample failed starting MLCamera, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
        }
    }
    
    private void DisableMLCamera()
    {
        lock (_cameraLockObject)
        {
            if (MLCamera.IsStarted)
            {
                MLCamera.Disconnect();
                // Explicitly set to false here as the disconnect was attempted.
                _isCameraConnected = false;
                MLCamera.Stop();
            }
        }
    }
    
    private void StartCapture()
    {
        if (!_hasStarted)
        {
            lock (_cameraLockObject)
            {
                EnableMLCamera();
                MLCamera.OnRawImageAvailable += OnCaptureRawImageComplete;
            }
            MLInput.OnControllerButtonDown += OnButtonDown;

            _hasStarted = true;
        }
    }
    
    private void HandlePrivilegesDone(MLResult result)
    {
        if (!result.IsOk)
        {
            if (result.Code == MLResultCode.PrivilegeDenied)
            {
                Instantiate(Resources.Load("PrivilegeDeniedError"));
            }

            Debug.LogErrorFormat("Error: ImageCaptureExample failed to get requested privileges, disabling script. Reason: {0}", result);
            enabled = false;
            return;
        }

        Debug.Log("Succeeded in requesting all privileges");
        StartCapture();
    }
    
    /// <summary>
    /// Handles the event of a new image getting captured.
    /// </summary>
    /// <param name="imageData">The raw data of the image.</param>
    private void OnCaptureRawImageComplete(byte[] imageData)
    {
        lock (_cameraLockObject)
        {
            _isCapturing = false;
        }
        // Initialize to 8x8 texture so there is no discrepency
        // between uninitalized captures and error texture
        Texture2D texture = new Texture2D(8, 8);
        bool status = texture.LoadImage(imageData);

        if (status && (texture.width != 8 && texture.height != 8))
        {
            OnImageCaptured(texture);
        }
    }

    /// <summary>
    /// Worker function to call the API's Capture function
    /// </summary>
    private void CaptureThreadWorker()
    {
        lock (_cameraLockObject)
        {
            if (MLCamera.IsStarted && _isCameraConnected)
            {
                MLResult result = MLCamera.CaptureRawImageAsync();
                if (result.IsOk)
                {
                    _isCapturing = true;
                }
                else if (result.Code == MLResultCode.PrivilegeDenied)
                {
                    _doPrivPopup = true;
                }
            }
        }
    }
    
}
