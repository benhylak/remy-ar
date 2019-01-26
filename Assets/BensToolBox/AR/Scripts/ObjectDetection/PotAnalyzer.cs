using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using StoveRecognizer;
using UnityEngine;
using UnityEngine.Networking;

public class PotAnalyzer : MonoBehaviour {

	/// <summary>
	/// Unique instance of this class
	/// </summary>
	public static PotAnalyzer Instance;

	/// <summary>
	/// Insert your prediction key here
	/// </summary>
	private string predictionKey = "3b47e5b137554e4fbbc5ef9710bf2e50";

	/// <summary>
	/// Insert your prediction endpoint here
	/// </summary>
	private string predictionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/e7cc1d6e-7666-4f13-b6ef-0bc87b451fc3/image?iterationId=281c19cf-9d25-49f3-95cf-0689bcf804d1";

	/// <summary>
	/// Bite array of the image to submit for analysis
	/// </summary>
	[HideInInspector] public byte[] imageBytes;

	private void Awake()
	{
		// Allows this instance to behave like a singleton
		Instance = this;
	}
	
	/// <summary>
    /// Call the Computer Vision Service to submit the image.
    /// </summary>
    public IEnumerator AnalyseLastImageCaptured(string imagePath)
    {
        Debug.Log("Analyzing...");

        WWWForm webForm = new WWWForm();

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(predictionEndpoint, webForm))
        {
            // Gets a byte array out of the saved image
            imageBytes = GetImageAsByteArray(imagePath);

            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("Prediction-Key", predictionKey);

            // The upload handler will help uploading the byte array with the request
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            // The download handler will help receiving the analysis from Azure
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return unityWebRequest.SendWebRequest();

            string jsonResponse = unityWebRequest.downloadHandler.text;

            Debug.Log("response");
            Debug.Log(jsonResponse);

            // Create a texture. Texture size does not matter, since
            // LoadImage will replace with the incoming image size.
         //    Texture2D tex = new Texture2D(1, 1);
            
            if (imageBytes == null)

            {
	            Debug.Log("ALERT: ImageBytes is null");
            }
            else
            {
	            Debug.Log("ImageBytes are not null!");
	            
            }
            
//            tex.LoadImage(imageBytes);
//            
//            
//            Debug.Log("Image loaded!");
//            
//            SceneOrganizer.Instance.quadRenderer.material.SetTexture("_MainTex", tex);

            // The response will be in JSON format, therefore it needs to be deserialized
            CustomVisionObjects.AnalysisRootObject analysisRootObject = new CustomVisionObjects.AnalysisRootObject();
            analysisRootObject = JsonConvert.DeserializeObject<CustomVisionObjects.AnalysisRootObject>(jsonResponse);

            PotRecognitionManager.Instance.FinaliseLabel(analysisRootObject);
        }
    }

    /// <summary>
    /// Returns the contents of the specified image file as a byte array.
    /// </summary>
    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);

        BinaryReader binaryReader = new BinaryReader(fileStream);

        return binaryReader.ReadBytes((int)fileStream.Length);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}