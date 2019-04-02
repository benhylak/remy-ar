using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BensToolBox.AR.Scripts
{
    public static class Extensions
    {
        public static IEnumerator WaitForKill(this Sequence seq)
        {
            yield return seq.WaitForElapsedLoops(1);
        }
        //waits and then runs on complete *on the main thread*
        public static async void DelayedInvokeOnMainThread(this object obj, float delaySeconds, Action onComplete) 
        { 
            await new WaitForSeconds(delaySeconds);
            await new WaitForUpdate();
            
            onComplete.Invoke();
        }
        
        public static void SetTransparency(this UnityEngine.UI.Image p_image, float p_transparency)
        {
            if (p_image != null)
            {
                UnityEngine.Color __alpha = p_image.color;
                __alpha.a = p_transparency;
                p_image.color = __alpha;
            }
        }


        public static float GetTransparency(this UnityEngine.UI.Image p_image)
        {
            if (p_image != null)
            {
                return p_image.color.a;
            }

            return 0;
        }
        
        public static void SetTransparency(this Renderer renderer, float p_transparency)
        {
            if (renderer != null)
            {
                UnityEngine.Color __alpha = renderer.material.color;
                __alpha.a = p_transparency;
                renderer.material.color = __alpha;
            }
        }

        public static float GetTransparency(this Renderer renderer)
        {
            if (renderer != null)
            {
                return renderer.material.color.a;
            }

            return 0;
        }
        
        public static float GetTransparency(this LineRenderer lineRenderer)
        {
            if (lineRenderer != null)
            {
                return lineRenderer.materials[0].GetTransparency();
            }

            return 0;
        }
        
        public static void SetTransparency(this LineRenderer lineRenderer, float p_transparency)
        {
            if (lineRenderer != null)
            {
                lineRenderer.materials[0].SetTransparency(p_transparency);
                lineRenderer.materials[0].SetEmissionGlow(Mathf.Lerp(0, 1.4f, p_transparency));
            }
        }
        
        public static float GetTransparency(this Material mat)
        {
            if (mat != null)
            {
                return mat.color.a;
            }

            return 0;
        }
        
        public static void SetTransparency(this Material mat, float p_transparency)
        {
            if (mat != null)
            {
                UnityEngine.Color __alpha = mat.color;
                __alpha.a = p_transparency;
                mat.color = __alpha;
            }
        }

        public static void SetEmissionGlow(this Material mat, float glow)
        {
            mat.SetColor("_EmissionColor", mat.color * glow);
        }
    }
}