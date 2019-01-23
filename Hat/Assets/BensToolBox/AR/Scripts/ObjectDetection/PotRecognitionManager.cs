using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StoveRecognizer;
using UnityEditor;

[RequireComponent(typeof(PotAnalyzer))]
public class PotRecognitionManager : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static PotRecognitionManager Instance;

    /// <summary>
    /// The cursor object attached to the Main Camera
    /// </summary>
    public GameObject cursor;

    /// <summary>
    /// The label used to display the analysis on the objects in the real world
    /// </summary>
    public GameObject label;

    /// <summary>
    /// Reference to the last Label positioned
    /// </summary>
    internal Transform lastLabelPlaced;

    /// <summary>
    /// Reference to the last Label positioned
    /// </summary>
    internal TextMesh lastLabelPlacedText;

    /// <summary>
    /// Current threshold accepted for displaying the label
    /// Reduce this value to display the recognition more often
    /// </summary>
    internal float probabilityThreshold = 0.35f;
    
    public Camera _Camera;

    public GameObject _sphere;

    public Camera _ReprojectionCamera;
    
    /// <summary>
    /// Called on initialization
    /// </summary>
    private void Awake()
    {
        // Use this class instance as singleton
        Instance = this;

        // Add the CustomVisionObjects class to this Gameobject
        gameObject.AddComponent<CustomVisionObjects>();
    }
    
    /// <summary>
    /// Instantiate a Label in the appropriate location relative to the Main Camera.
    /// </summary>
    public void PlaceAnalysisLabel()
    {
        _ReprojectionCamera.transform.position = _Camera.transform.position;
        _ReprojectionCamera.transform.rotation = _Camera.transform.rotation;
        
        _sphere.transform.position = cursor.transform.position;
        
        lastLabelPlaced = Instantiate(label.transform, cursor.transform.position, transform.rotation);
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
        lastLabelPlacedText.text = "";
        lastLabelPlaced.transform.localScale = new Vector3(0.005f,0.005f,0.005f);
    }
    
     /// <summary>
    /// Set the Tags as Text of the last label created. 
    /// </summary>
    public void FinaliseLabel(CustomVisionObjects.AnalysisRootObject analysisObject)
    {
        if (analysisObject.predictions != null)
        {
            lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
            // Sort the predictions to locate the highest one
            List<CustomVisionObjects.Prediction> sortedPredictions = new List<CustomVisionObjects.Prediction>();
            sortedPredictions = analysisObject.predictions.OrderBy(p => p.probability).ToList();
            CustomVisionObjects.Prediction bestPrediction = new CustomVisionObjects.Prediction();
            bestPrediction = sortedPredictions[sortedPredictions.Count - 1];

            if (bestPrediction.probability > probabilityThreshold)
            {              
                var centerY = (float) (bestPrediction.boundingBox.top - bestPrediction.boundingBox.height / 2);
                
                var centerX = (float) (bestPrediction.boundingBox.left + bestPrediction.boundingBox.width / 2);

                //reversed for viewport coords
                var projectedRay = _ReprojectionCamera.ViewportPointToRay(new Vector3(centerX, 1-centerY, 0));

                RaycastHit hit;
                
                lastLabelPlacedText.text = bestPrediction.tagName;
                lastLabelPlacedText.transform.position = _sphere.transform.position;
             
                if (Physics.Raycast(projectedRay, out hit, 20.0f, 1 << LayerMask.NameToLayer("World")))
                {
                    lastLabelPlaced.transform.position = hit.point;
                    _sphere.transform.position = lastLabelPlaced.transform.position;
                }           
            }
            
            cursor.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            cursor.GetComponent<Renderer>().material.color = Color.yellow;
        }
        // Reset the color of the cursor
    }
}
