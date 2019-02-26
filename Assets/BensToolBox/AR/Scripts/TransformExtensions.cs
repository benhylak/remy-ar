using UnityEngine;

namespace BensToolBox.AR.Scripts
{
    public static class TransformExtensions {
        
        public static void SetLocalPosX(this Transform t, float newXPos) {
            var pos = t.localPosition;
            pos.x = newXPos;
            t.localPosition = pos;
        }
        
        public static void SetLocalPosY(this Transform t, float newYPos) {
            var pos = t.localPosition;
            pos.y = newYPos;
            t.localPosition = pos;
        }
        
        public static void SetLocalPosZ(this Transform t, float newZPos) {
            var pos = t.localPosition;
            pos.z = newZPos;
            t.localPosition = pos;
        }

        
        public static void SetPosX(this Transform t, float newXPos) {
            var pos = t.position;
            pos.x = newXPos;
            t.position = pos;
        }
        
        public static void SetPosY(this Transform t, float newYPos) {
            var pos = t.position;
            pos.y = newYPos;
            t.position = pos;
        }
        
        public static void SetPosZ(this Transform t, float newZPos) {
            var pos = t.position;
            pos.z = newZPos;
            t.position = pos;
        }
        
        public static void SetAnchoredPosX(this RectTransform t, float newXPos) {
            var pos = t.anchoredPosition;
            pos.x = newXPos;
            t.anchoredPosition = pos;
        }
        
        public static void SetAnchoredPosY(this RectTransform t, float newYPos) {
            var pos = t.anchoredPosition;
            pos.y = newYPos;
            t.anchoredPosition = pos;
        }
    }
}