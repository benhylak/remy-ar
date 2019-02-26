using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{
    public abstract class MoveWithControllerBehaviour : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("controller") && MLInput.GetController(0).TriggerValue > MLInput.TriggerDownThreshold)
            {
                MoveWithController(other.gameObject);
            }
        }

        public void MoveWithController(GameObject controller)
        {
            this.transform.position = controller.transform.position;

            var lookVec = -controller.transform.forward;
            this.transform.forward = new Vector3(lookVec.x, lookVec.y, 0);
        }

        private void OnTriggerExit(Collider other)
        {
            //drop it down and animate it.
        }
    }
}