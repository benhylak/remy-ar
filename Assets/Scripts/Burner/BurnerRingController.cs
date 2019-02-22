
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine;

public class BurnerRingController : MonoBehaviour
{
    public Material VoiceInputMat;
    public Material WhiteProactive;

    private Color voicePrimaryColor;
    private Color voiceSecondaryColor;
	
    private string PRIMARY_COLOR_VOICE = "_PrimaryColor";
    private string SECONDARY_COLOR_VOICE = "_SecondaryColor";
    
    private static readonly float MAX_VOLUME = 0.15f;
    private float totalAmt = 0;
    	
    public float WAVE_AMPLITUDE = 0.005f;

    public float RING_RADIUS = 0.1f;
    
    private float lerpAmt;

    private Renderer _renderer;
    
    public void Start()
    {
        voicePrimaryColor = VoiceInputMat.GetColor(PRIMARY_COLOR_VOICE);
        voiceSecondaryColor = VoiceInputMat.GetColor(SECONDARY_COLOR_VOICE);
        _renderer = GetComponent<Renderer>();
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
	
	
    public void SetRingRadius(float radius)
    {
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