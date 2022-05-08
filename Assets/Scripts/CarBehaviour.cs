using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarBehaviour : MonoBehaviour
{
    public WheelBehaviour[] wheelBehaviours = new WheelBehaviour[4];
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;
    private string _groundTagFL;
    private string _groundTagFR;
    private int _groundTextureFL = 0;
    private int _groundTextureFR = 0;
    
    public Material buggyColor;
    public Color buggyBodyColor;

    public float maxTorque = 2000;
    public float maxBrakeTorque = 15000;
    public float maxSteerAngleDEG = 40;

    public float sidewaysStiffness = 1.5f;
    public float forewardStiffness = 2.5f;

    private float _currentSpeedKMH;
    public float _maxSteerAngleHighSpeed = 20;
    private Rigidbody _rigidbody;
    public Transform centerOfMass;

    public float maxSpeedKMH = 120;
    public float maxSpeedBackwardKMH = 30;

    public RectTransform speedPointerTransform;
    public TMP_Text speedText;

    float stopAngle = -34;
    float topSpeedAngle = -326;

    public AudioClip engineSingleRPMSoundClip;
    private AudioSource _engineAudioSource;

    public ParticleSystem smoke;
    private ParticleSystem.EmissionModule _smokeEmission;

    public bool thrustEnabled = false;
    
    private Prefs _prefs;

    
    // Full breaking and skidmarking
    public float fullBrakeTorque = 5000;
    public AudioClip brakeAudioClip;
    private bool _doSkidmarking;
    private bool _carIsOnDrySand;
    private bool _carIsNotOnSand;
    private AudioSource _brakeAudioSource;

    void Start() {

        _prefs = new Prefs();
        _prefs.Load();
        _prefs.SetAll(ref wheelColliderFL, ref wheelColliderFR,
            ref wheelColliderRL, ref wheelColliderRR, ref buggyColor, ref buggyBodyColor);
        
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = new Vector3(centerOfMass.localPosition.x,
        centerOfMass.localPosition.y,
        centerOfMass.localPosition.z);
        
        //buggy less sliperry
        
        
        // Configure AudioSource component by program
        _engineAudioSource = gameObject.AddComponent<AudioSource>();
        _engineAudioSource.clip = engineSingleRPMSoundClip;
        _engineAudioSource.loop = true;
        _engineAudioSource.volume = 0.7f;
        _engineAudioSource.playOnAwake = true;
        _engineAudioSource.enabled = false; // Bugfix
        _engineAudioSource.enabled = true; // Bugfix

        _smokeEmission = smoke.emission;
        _smokeEmission.enabled = true;
        
        // Configure brake audiosource component by program
        _brakeAudioSource = (AudioSource)gameObject.AddComponent<AudioSource>();
        _brakeAudioSource.clip = brakeAudioClip;
        _brakeAudioSource.loop = true;
        _brakeAudioSource.volume = 0.7f;
        _brakeAudioSource.playOnAwake = false;
        
        SetWheelFrictionStiffness(forewardStiffness,sidewaysStiffness);
    }



    void FixedUpdate()
    {
        // get the current speed from the velocity vector
        _currentSpeedKMH = _rigidbody.velocity.magnitude * 3.6f;
        int gearNum = 0;
        float engineRPM = kmh2rpm(_currentSpeedKMH, out gearNum);
        // Determine if the car is driving forwards or backwards
        float angleToForward = Vector3.Angle(transform.forward, _rigidbody.velocity);
        bool velocityIsForeward = angleToForward < 45f;
 
       
        // set the steer angle on the front wheels depending on the speed.
        float steerReduction = Mathf.Max(1.0f - _currentSpeedKMH / 50.0f, 0.3f);
        float steerAngle = maxSteerAngleDEG * steerReduction * Input.GetAxis("Horizontal");
        SetSteerAngle(steerAngle);
        
        // Evaluate ground under front wheels
        WheelHit hitFL = GetGroundInfos(ref wheelColliderFL,ref _groundTagFL,ref _groundTextureFL);
        WheelHit hitFR = GetGroundInfos(ref wheelColliderFR,ref _groundTagFR,ref _groundTextureFR);
        _carIsOnDrySand = _groundTagFL.CompareTo("Terrain")==0 && _groundTextureFL==1;
        _carIsNotOnSand = !(_groundTagFL.CompareTo("Terrain")==0 && (_groundTextureFL<=1));
        
        
        bool doBraking = _currentSpeedKMH > 0.5f &&
                         (Input.GetAxis("Vertical") < 0 && velocityIsForeward ||
                          Input.GetAxis("Vertical") > 0 && !velocityIsForeward);
        bool doFullBrake = Input.GetKey("space");
        _doSkidmarking = _carIsNotOnSand && doFullBrake && _currentSpeedKMH > 20.0f;
        SetSkidmarking(_doSkidmarking);
        
        if (doBraking || doFullBrake)
        {
            float brakeTorque = doFullBrake ? maxBrakeTorque : fullBrakeTorque;
            SetBrakeTorque(brakeTorque);
            SetMotorTorque(0);
        }
        else
        {
            SetBrakeTorque(0);
            if(thrustEnabled && velocityIsForeward && _currentSpeedKMH < maxSpeedKMH || 
                thrustEnabled && !velocityIsForeward && _currentSpeedKMH < maxSpeedBackwardKMH)
            {
                float torque = maxTorque * Input.GetAxis("Vertical");
                SetMotorTorque(torque);
            }
            else
            {
                SetMotorTorque(0);
            }
        }
        
        
        SetBrakeSound(_doSkidmarking);
        SetEngineSound(engineRPM);
        SetParticleSystems(engineRPM);
    }

    void SetBrakeSound(bool doBrakeSound)
    {
        if (doBrakeSound)
        { _brakeAudioSource.volume = _currentSpeedKMH/100.0f;
            _brakeAudioSource.Play();
        } else
            _brakeAudioSource.Stop();
    }
    void SetParticleSystems(float engineRPM)
    {
        float smokeRate = engineRPM / 50.0f;
        _smokeEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);
    }


    void SetEngineSound(float engineRPM)
    {
        if (_engineAudioSource == null) return;
        float minRPM = 800;
        float maxRPM = 8000;
        float minPitch = 0.3f;
        float maxPitch = 3.0f;
        float pitch = 1.0f;
        if (engineRPM >= minRPM && engineRPM <= maxRPM)
        {
            pitch = Mathf.Lerp(minPitch, maxPitch, engineRPM / maxRPM);
        }
        else
        {
            pitch = minRPM;
        }
        _engineAudioSource.pitch = pitch;
    }

    void SetSteerAngle(float angle)
    {
        wheelColliderFL.steerAngle = angle;
        wheelColliderFR.steerAngle = angle;
        
    }
    void SetMotorTorque(float amount)
    {
        wheelColliderFL.motorTorque = amount;
        wheelColliderFR.motorTorque = amount;
    }

    void SetBrakeTorque(float amount)
    {
        wheelColliderFL.brakeTorque = amount;
        wheelColliderFR.brakeTorque = amount;
        wheelColliderRL.brakeTorque = amount;
        wheelColliderRR.brakeTorque = amount;
    }
    void SetWheelFrictionStiffness(float newForewardStiffness, float newSidewaysStiffness)
    {
        WheelFrictionCurve fwWFC = wheelColliderFL.forwardFriction;
        WheelFrictionCurve swWFC = wheelColliderFL.sidewaysFriction;
        fwWFC.stiffness = newForewardStiffness;
        swWFC.stiffness = newSidewaysStiffness;
        wheelColliderFL.forwardFriction = fwWFC;
        wheelColliderFL.sidewaysFriction = swWFC;
        wheelColliderFR.forwardFriction = fwWFC;
        wheelColliderFR.sidewaysFriction = swWFC;
        wheelColliderRL.forwardFriction = fwWFC;
        wheelColliderRL.sidewaysFriction = swWFC;
        wheelColliderRR.forwardFriction = fwWFC;
        wheelColliderRR.sidewaysFriction = swWFC;
    }
    // Turns skidmarking on or off on all wheels
    void SetSkidmarking(bool doSkidmarking)
    { foreach(var wheel in wheelBehaviours)
        wheel.DoSkidmarking(doSkidmarking);
    }

    void OnGUI()
    {
        // Speedpointer rotation
        //if (_currentSpeedKMH == 0) { float degAroundZ = -34; }
        //else { float degAroundZ = _currentSpeedKMH / maxSpeedKMH - 34; }

        float speedFraction = _currentSpeedKMH / maxSpeedKMH;
        float degAroundZ = 0;
        if (speedFraction < 0)
        {
            degAroundZ = (-1) * (Mathf.Lerp(stopAngle, topSpeedAngle, speedFraction));
        }
        else
        {
            degAroundZ = Mathf.Lerp(stopAngle, topSpeedAngle, speedFraction);
        }
        speedPointerTransform.rotation = Quaternion.Euler(0, 0, degAroundZ);
        // SpeedText show current KMH
        speedText.text = _currentSpeedKMH.ToString("0") + " km/h";
    }

    class Gear
    {
        public Gear(float minKMH, float minRPM, float maxKMH, float maxRPM)
        {
            _minRPM = minRPM;
            _minKMH = minKMH;
            _maxRPM = maxRPM;
            _maxKMH = maxKMH;
        }
        private float _minRPM;
        private float _minKMH;
        private float _maxRPM;
        private float _maxKMH;
        public bool speedFits(float kmh)
        {
            return kmh >= _minKMH && kmh <= _maxKMH;
        }
        public float interpolate(float kmh)
        { 
            if(_maxRPM * kmh/_maxKMH > _minRPM)
            {
                return _maxRPM * kmh / _maxKMH;
            }
            return _minRPM; 

        }
    }
    float kmh2rpm(float kmh, out int gearNum)
    {
        Gear[] gears =
            { 
                new Gear( 1, 900, 12, 1400),
                new Gear( 12, 900, 25, 2000),
                new Gear( 25, 1350, 45, 2500),
                new Gear( 45, 1950, 70, 3500),
                new Gear( 70, 2500, 112, 4000),
                new Gear(112, 3100, 180, 5000)
            };
        for (int i = 0; i < gears.Length; ++i)
        {
            if (gears[i].speedFits(kmh))
            {
                gearNum = i + 1;
                return gears[i].interpolate(kmh);
            }
        }
        gearNum = 1;
        return 800;
    }
    WheelHit GetGroundInfos(ref WheelCollider wheelCol,
        ref string groundTag,
        ref int groundTextureIndex)
    { // Default values
        groundTag = "InTheAir";
        groundTextureIndex = -1;
// Query ground by ray shoot on the front left wheel collider
        WheelHit wheelHit;
        wheelCol.GetGroundHit(out wheelHit);
// If not in the air query collider
        if (wheelHit.collider)
        { groundTag = wheelHit.collider.tag;
            if (wheelHit.collider.CompareTag("Terrain"))
                groundTextureIndex = TerrainSurface.GetMainTexture(transform.position);
        }
        return wheelHit;
    }
}