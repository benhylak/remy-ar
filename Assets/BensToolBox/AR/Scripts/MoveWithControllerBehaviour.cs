using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{
    public class MoveWithControllerBehaviour : MonoBehaviour
    {
        protected MLWorldRays.QueryParams _raycastParams = new MLWorldRays.QueryParams();
        public Sequence _placeTween;
        public bool isMoving;

        private Transform _originalParent;

        private void OnTriggerStay(Collider other)
        {
            if (!isMoving && other.gameObject.CompareTag("controller") &&
                MLInput.GetController(0).TriggerValue > MLInput.TriggerDownThreshold)
            {
                _originalParent = gameObject.transform.parent;
                gameObject.transform.SetParent(other.gameObject.transform);
                isMoving = true;
                _placeTween?.Kill();
            }
        }

        public void Update()
        {
            if (isMoving &&  MLInput.GetController(0).TriggerValue < MLInput.TriggerUpThreshold)
            {
                isMoving = false;
                Place();
            }
        }

        public void Place()
        {
            _raycastParams.Position = gameObject.transform.position;
            _raycastParams.Direction = Vector3.down;
                      
            gameObject.transform.SetParent(_originalParent);
            
            MLWorldRays.GetWorldRays(_raycastParams, HandleOnReceiveRaycast);
        }

        public void MoveWithController(GameObject controller)
        {
           // this.transform.position = controller.transform.position + _grabOffset;

          //  this.transform.rotation = controller.transform.rotation * Quaternion.Inverse(_rotOffset);
        }
        
        protected RaycastHit GetWorldRaycastResult(MLWorldRays.MLWorldRaycastResultState state, Vector3 point, Vector3 normal, float confidence)
        {
            RaycastHit result = new RaycastHit();

            if (state != MLWorldRays.MLWorldRaycastResultState.RequestFailed && state != MLWorldRays.MLWorldRaycastResultState.NoCollision)
            {
                result.point = point;
                result.normal = normal;
                result.distance = Vector3.Distance(_raycastParams.Position, point);
            }

            return result;
        }        

        /// <summary>
        /// Callback handler called when raycast call has a result.
        /// </summary>
        /// <param name="state"> The state of the raycast result.</param>
        /// <param name="point"> Position of the hit.</param>
        /// <param name="normal"> Normal of the surface hit.</param>
        /// <param name="confidence"> Confidence value on hit.</param>
        protected void HandleOnReceiveRaycast(MLWorldRays.MLWorldRaycastResultState state, Vector3 point, Vector3 normal, float confidence)
        {
            RaycastHit result = GetWorldRaycastResult(state, point, normal, confidence);

            _placeTween?.Kill();

            if (state != MLWorldRays.MLWorldRaycastResultState.NoCollision)
            {
                
                _placeTween = DOTween.Sequence();
                
                _placeTween.Append(
                    gameObject.transform
                        .DOMove(result.point, 0.6f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => _placeTween = null));

                var endRot = gameObject.transform.rotation.eulerAngles;
                endRot.x = 0;
                endRot.z = 0;
            
                _placeTween.Insert(0,
                    gameObject.transform.DORotate(
                        endRot, 0.6f));
            }
        }

//        private void OnTriggerExit(Collider other)
//        {
//            if (isMoving)
//            {
//                isMoving = false;
//                Place();
//            }
//        }
    }
}