using UnityEngine;
public class Prefs
{
    public float suspensionDistance;
    public float hue;
    public float saturation;
    public float luminance;
    public float score;
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    public void Load()
    { 
        suspensionDistance = PlayerPrefs.GetFloat("suspensionDistance", 0.2f);
        hue = PlayerPrefs.GetFloat("hue", 0);
        saturation = PlayerPrefs.GetFloat("saturation", 1);
        luminance = PlayerPrefs.GetFloat("luminance", 1);
        score = PlayerPrefs.GetFloat("score", 0);
    }
    public void Save()
    { 
        PlayerPrefs.SetFloat("suspensionDistance", suspensionDistance); 
        PlayerPrefs.SetFloat("hue", hue); 
        PlayerPrefs.SetFloat("saturation", saturation); 
        PlayerPrefs.SetFloat("luminance", luminance); 
        PlayerPrefs.SetFloat("score", score);
    }
    public void SetAll(ref WheelCollider wheelFL, ref WheelCollider wheelFR,
        ref WheelCollider wheelRL, ref WheelCollider wheelRR, ref Material buggyColor, ref Color buggyBodyColor)
    { 
        SetWheelColliderSuspension(ref wheelFL, ref wheelFR,
        ref wheelRL, ref wheelRR);
        SetBuggyColor(ref buggyColor);
    }
    public void SetWheelColliderSuspension(ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    { wheelFL.suspensionDistance = suspensionDistance;
        wheelFR.suspensionDistance = suspensionDistance;
        wheelRL.suspensionDistance = suspensionDistance;
        wheelRR.suspensionDistance = suspensionDistance;
    }

    public void SetBuggyColor(ref Material buggyColor)
    {
         buggyColor.SetColor(Color1,Color.HSVToRGB(hue, saturation, luminance));
    }
}