using BensToolBox.AR.Scripts;
using MagicLeap;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class NotificationTestManager : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer eyeRaycastVisualizer;

    [SerializeField]
    private HeadsUpNotification headsUpNotification;

    [SerializeField]
    private PhysicalNotification physicalNotification;

    private float triggerDownStartTime = -1;
    private float startExperimentTriggerDownThreshold = 0.5f;
    
    public void Start()
    {
        var gestureObserver = gameObject.AddComponent<ControllerGestureObserver>();

        gestureObserver.OnLongTriggerDown().Subscribe(_ =>
        {
            StartExperiment();
        });
        
        MLInput.OnControllerButtonDown += (b, button) =>
        {
            if (button == MLInputControllerButton.Bumper)
            {
                eyeRaycastVisualizer.enabled = !eyeRaycastVisualizer.enabled;
            }
        };

        headsUpNotification.gameObject.GetComponent<GazeReceiver>().OnGazeExit.Subscribe(duration =>
        {
            Debug.LogError("Gaze Exit: Lasted " + duration + " seconds.");
        });

//        headsUpNotification.gameObject.GetComponent<GazeReceiver>().OnGazeEnter.Subscribe(_ =>
//        {
//            Debug.LogError("test");
//        });
        
        
        MLInput.OnTriggerDown += (b, f) => { headsUpNotification.Launch(); };
    }

    public void StartExperiment()
    {
        //new experiment entry
        //
    }

    private void Update()
    {
       
    }
}