using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace DefaultNamespace
{
    public class FingerRaycast : BaseRaycast
    {
        #region Private Variables

        private Vector3 _fingerPosition;
        
        #endregion

        #region Protected Properties
        /// <summary>
        /// Returns the position of current headpose.
        /// </summary>
        override protected Vector3 Position
        {
            get { return _fingerPosition; }
        }

        /// <summary>
        /// Returns the direction of current headpose.
        /// </summary>
        override protected Vector3 Direction
        {
            get { return Vector3.down; }
        }

        /// <summary>
        /// Returns the up vector of current finger
        /// </summary>
        override protected Vector3 Up
        {
            get { return Vector3.up; }
        }
        #endregion

        public void UpdatePosition(Vector3 position)
        {
            _fingerPosition = position;
        }

        #region Unity Methods
        /// <summary>
        /// Initialize variables.
        /// </summary>
        void Awake()
        {
           
        }
        #endregion
    }
}