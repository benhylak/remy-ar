using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{
    public class GazeRaycaster : MonoBehaviour
    {
        public Camera _camera;
       
     /*   [System.Serializable]
        public class RaycastResultEvent : UnityEvent<RaycastHit> { }
        
        public RaycastResultEvent OnRaycastHit;*/
        
        
        protected Vector3 Position => _camera.transform.position;

        /// <summary>
        /// Returns the direction of headpose to eye fixation point.
        /// </summary>
        protected Vector3 Direction => (MLEyes.FixationPoint - _camera.transform.position).normalized;

        /// <summary>
        /// Returns the up vector of current headpose.
        /// </summary>
        protected Vector3 Up
        {
            get
            {
                return _camera.transform.up;
            }
        }

        private void Update()
        {
            Ray ray;
            // Virtual Raycast
            RaycastHit result;
            float maxDist = 20f;
            
            # if UNITY_EDITOR
            
            ray = _camera.ScreenPointToRay(Input.mousePosition);
   
            #else             
            ray = new Ray(Position, Direction);
                
            # endif
            
            if (Physics.Raycast(ray, out result, maxDist))
            {
                var gazeReceiver = result.collider.gameObject.GetComponent<GazeReceiver>();
    
                if (gazeReceiver != null)
                {
                    gazeReceiver.OnLook();
                }
            }
        }
    }
}