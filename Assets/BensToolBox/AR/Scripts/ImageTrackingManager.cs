// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{

    public class ImageTrackingManager : MonoBehaviour
    {
        public GameObject[] TrackerBehaviours;

        private bool _hasStarted = false;

        #region Unity Methods

        private void Awake()
        {
            StartCapture();
        }

        /// <summary>
        /// Cannot make the assumption that a privilege is still granted after
        /// returning from pause. Return the application to the state where it
        /// requests privileges needed and clear out the list of already grantedf
        /// privileges. Also, unregister callbacks.
        /// </summary>
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                UpdateImageTrackerBehaviours(false);

                _hasStarted = false;
            }
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Control when to enable to image trackers based on
        /// if the correct privileges are given.
        /// </summary>
        void UpdateImageTrackerBehaviours(bool enabled)
        {
            foreach (GameObject obj in TrackerBehaviours)
            {
                obj.SetActive(enabled);
            }
        }

        /// <summary>
        /// Once privileges have been granted, enable the camera and callbacks.
        /// </summary>
        void StartCapture()
        {
            if (!_hasStarted)
            {
                UpdateImageTrackerBehaviours(true);

                _hasStarted = true;
            }
        }

        #endregion

        /// <summary>
        /// Responds to privilege requester result.
        /// </summary>
        /// <param name="result"/>
        void HandlePrivilegesDone(MLResult result)
        {
            if (!result.IsOk)
            {
                if (result.Code == MLResultCode.PrivilegeDenied)
                {
                    Instantiate(Resources.Load("PrivilegeDeniedError"));
                }

                Debug.LogErrorFormat(
                    "Error: ImageTrackingExample failed to get requested privileges, disabling script. Reason: {0}",
                    result);
                enabled = false;
                return;
            }

            Debug.Log("Succeeded in requesting all privileges");
            StartCapture();
        }
        
        public void Disable()
        {
            if (_hasStarted && MLImageTracker.IsStarted)
            {
                MLImageTracker.Disable();
            }
        }

        public void Enable()
        {
            if (_hasStarted && MLImageTracker.IsStarted)
            {
                MLImageTracker.Enable();
            }
        }
    }
    
}