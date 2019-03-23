
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine;

public class BurnerRingController : MonoBehaviour
{
    public Material VoiceInputMat;
    public Material WhiteProactive;
    public Material BoilingWaitMaterial;

    private Color voicePrimaryColor;
    private Color voiceSecondaryColor;
	
    private string PRIMARY_COLOR_VOICE = "_PrimaryColor";
    private string SECONDARY_COLOR_VOICE = "_SecondaryColor";
    
    private static readonly float MAX_VOLUME = 0.15f;
    private float totalAmt = 0;
    	
    public float WAVE_AMPLITUDE = 0.005f;

    public float RING_RADIUS = 0.1f;
    
    private float lerpAmt;

    public Renderer _renderer;
    
    public void Start()
    {
        voicePrimaryColor = VoiceInputMat.GetColor(PRIMARY_COLOR_VOICE);
        voiceSecondaryColor = VoiceInputMat.GetColor(SECONDARY_COLOR_VOICE);
    }

    public Tween Show(float duration = 0.3f)
    {
        return DOTween.To(GetAlpha, SetAlpha, 1f, duration)
            .SetEase(Ease.InSine);
    }
    
    public Tween Hide(float duration = 0.3f)
    {
        return DOTween.To(GetAlpha, SetAlpha, 0f, duration)
            .SetEase(Ease.OutSine);
    }
    
    public void SetWaveAmplitude(float amt)
    {
        _renderer.material.SetFloat("_WaveAmp", amt);
    }

    public float GetWaveAmplitude()
    {
        return _renderer.material.GetFloat("_WaveAmp");
    }

    public void SetMaterialToDefault()
    {
        _renderer.material = WhiteProactive;
    }

    public void SetMaterialToVoiceInput()
    {
        _renderer.material = VoiceInputMat;
    }
    public void SetMaterialToBoiling()
    {
        GetComponent<Renderer>().material = BoilingWaitMaterial;
    }

    public void SetColor(Color c)
    {
        _renderer.material.SetColor("_Color", c);
        _renderer.material.SetColor("_RimColor", c);
    }
    
    public void SetInputLevel(float level)
    {
        //catch exceptions here 
        
        var volumeMapped = Utility.Map(level, 0, MAX_VOLUME, 0, 1, clamp: true);
        var currentLevel = _renderer.material.GetFloat("_Volume");

        if (_renderer.material.HasProperty("_Volume"))
        {
            _renderer.material.SetFloat("_Volume",
                Mathf.Lerp(currentLevel, (float) volumeMapped, 0.08f));
        }
    }

    /**
     * Set Alpha. Only has an effect if the material has an alpha property
     */
    public void SetAlpha(float a)
    {
        if (_renderer.material.HasProperty("_Alpha"))
        {
            _renderer.material.SetFloat("_Alpha", a);
        }
    }

    /*
    * Get Alpha. Returns -1 if the material does not have an alpha property
    */
    public float GetAlpha()
    {
        if (_renderer.material.HasProperty("_Alpha"))
        {
            return _renderer.material.GetFloat("_Alpha");
        }

        return -1;
    }
	
	
    public void SetRingRadius(float radius)
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        
        _renderer.material.SetFloat("_Radius", radius);
    }
	
    public float GetRingRadius()
    {
        return _renderer.sharedMaterial.GetFloat("_Radius");
    }
	
	
    public void SetVoiceLerp(float amt)
    {
        lerpAmt = amt;
		
        _renderer.material.SetColor(PRIMARY_COLOR_VOICE, Color.Lerp(Color.white, voicePrimaryColor, amt));
        _renderer.material.SetColor(SECONDARY_COLOR_VOICE, Color.Lerp(Color.white, voiceSecondaryColor, amt));
    }

    public float GetVoiceLerp()
    {
        return lerpAmt;
    }
}