using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{
    /// <summary>
    /// Class for tracking a specific Keypose and handling confidence value
    public class KeyPoseTracker : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("KeyPose to track.")]
        private MLHandKeyPose _keyPoseToTrack = MLHandKeyPose.NoPose;

        [Space, SerializeField, Tooltip("Flag to specify if left hand should be tracked.")]
        private bool _trackLeftHand = true;

        [SerializeField, Tooltip("Flag to specify id right hand should be tracked.")]
        private bool _trackRightHand = true;

        public readonly float MIN_REQUIRED_CONFIDENCE = 0.55f;

        public Vector3 handPosition;

        public bool KeyposeActive = false;

        private List<float> handMovements = new List<float>();

        public float handVelocity;
        //private Vector3 lastRawHandPos;
        private float lastVelocityCheck;
        private static readonly float VELOCITY_CHECK_INTERVAL = 0.07f;
        private static readonly float MAXIMUM_STATIONARY_VELOCITY = 0.375f;

        private float _startedStationaryTime;
        public float lastStationaryTime;
        
        public float HasBeenStationaryForSeconds => IsHandStationary ? Time.time - _startedStationaryTime : 0;
        
        public bool IsHandStationary;
        
        public float keyposeConfidence;
        
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initialize variables.
        /// </summary>
        void Awake()
        {
          
        }

        /// <summary>
        /// Update color of sprite renderer material based on confidence of the KeyPose.
        /// </summary>
        void Update()
        {
            if (!MLHands.IsStarted)
            {            
                KeyposeActive = false;
                return;
            }

            float confidenceLeft = _trackLeftHand ? GetKeyPoseConfidence(MLHands.Left) : 0.0f;
            float confidenceRight = _trackRightHand ? GetKeyPoseConfidence(MLHands.Right) : 0.0f;

            MLHand bestHand = confidenceLeft > confidenceRight ?  MLHands.Left : MLHands.Right;

            keyposeConfidence = Mathf.Max(confidenceLeft, confidenceRight);
            
            if (bestHand.IsVisible && bestHand.KeyPoseConfidenceFiltered > MIN_REQUIRED_CONFIDENCE)
            {
                if (Time.time - lastVelocityCheck > VELOCITY_CHECK_INTERVAL)
                {
                    var latestVelocity = Vector3.Distance(handPosition, bestHand.Center) / (Time.time - lastVelocityCheck);

                    if (handVelocity == float.MaxValue) handVelocity = latestVelocity;
                    else
                    {                        
                        handVelocity = Mathf.Lerp(handVelocity, latestVelocity, 0.45f);
                        
                        var currentlyStationary = handVelocity < MAXIMUM_STATIONARY_VELOCITY;
            
                        if (currentlyStationary && !IsHandStationary)
                        {
                            _startedStationaryTime = Time.time;
                        }

                        IsHandStationary = currentlyStationary;
                    }
                    
                    lastVelocityCheck = Time.time;
                }
                
                if (!KeyposeActive)
                {
                    handPosition = bestHand.Center;
                    lastVelocityCheck = Time.time;
                    handVelocity = float.MaxValue;
                    IsHandStationary = false;
                    
                    KeyposeActive = true;
                }
                else
                {
                   
                    handPosition = Vector3.Lerp(handPosition, bestHand.Center, 4f * Time.smoothDeltaTime);
                }
            }
            else KeyposeActive = false;


            if (IsHandStationary)
            {
                lastStationaryTime = Time.time;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get the confidence value for the hand being tracked.
        /// </summary>
        /// <param name="hand">Hand to check the confidence value on. </param>
        /// <returns></returns>
        private float GetKeyPoseConfidence(MLHand hand)
        {
            if (hand != null)
            {
                if (hand.KeyPose == _keyPoseToTrack)
                {
                    return hand.KeyPoseConfidenceFiltered;
                }
            }
            return 0.0f;
        }
        #endregion
    }
}
