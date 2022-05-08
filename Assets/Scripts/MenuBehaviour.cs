using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MenuBehaviour : MonoBehaviour
{
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;
    public Material buggyColor;
    public Slider sliSuspDistance;
    public Slider sliHue;
    public Slider sliSaturation;
    public Slider sliLuminance;
    public Color buddyBodyColor;
    public Text txtDistanceNum;
    private Prefs _prefs;
    void Start()
    { 
        _prefs = new Prefs();
        _prefs.Load();
        _prefs.SetAll(ref wheelColliderFL, ref wheelColliderFR,
            ref wheelColliderRL, ref wheelColliderRR, ref buggyColor, ref buddyBodyColor);
        sliSuspDistance.value = _prefs.suspensionDistance;
        sliHue.value = _prefs.hue;
        sliSaturation.value = _prefs.saturation;
        sliLuminance.value = _prefs.luminance;
        txtDistanceNum.text = sliSuspDistance.value.ToString("0.00");
    }
    public void OnSliderChangedSuspDistance(float suspDistance)
    {
        txtDistanceNum.text = sliSuspDistance.value.ToString("0.00");
        _prefs.suspensionDistance = sliSuspDistance.value;
        _prefs.hue = sliHue.value;
        _prefs.saturation = sliSaturation.value;
        _prefs.luminance = sliLuminance.value;
        
        _prefs.SetBuggyColor(ref buggyColor);
        _prefs.SetWheelColliderSuspension(ref wheelColliderFL, ref wheelColliderFR,
            ref wheelColliderRL, ref wheelColliderRR);
    }

   
    public void OnBtnClickStart()
    { _prefs.Save();
        SceneManager.LoadScene("Scene1");
    }
    void OnApplicationQuit()
    { _prefs.Save();
    }
}