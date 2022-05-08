using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class TimingBehaviour : MonoBehaviour
{
    public int countMax = 3; 
    private int _countDown;
    private CarBehaviour _carScript;

    public TMP_Text timeText;

    public AudioClip countDownSoundClip;
    private AudioSource _countDownAudioSrc;
    public Button ReplayButon;
    public Button MenuButon;
    public Button LeaderBoardButon;
    

    private float _pastTime = 0;
    public bool isFinished = false;
    private bool _isStarted = false;
    
    private TimingBehaviour _gateStart;

    private Prefs _prefs;
    void Start()
    {
        _prefs = new Prefs();
        _prefs.Load();
        
        ReplayButon = GameObject.Find("Replay").GetComponent<Button>();
        MenuButon = GameObject.Find("Menu").GetComponent<Button>();
        LeaderBoardButon = GameObject.Find("LeaderBoard").GetComponent<Button>();
        ReplayButon.onClick.AddListener(StartGameAgain);
        MenuButon.onClick.AddListener(BackToMenu);
        LeaderBoardButon.onClick.AddListener(LeaderBoardMenu);
        // Configure AudioSource component by program
        _countDownAudioSrc = gameObject.AddComponent<AudioSource>();
        _countDownAudioSrc.clip = countDownSoundClip;
        _countDownAudioSrc.volume = 1.0f;
        _countDownAudioSrc.enabled = false; // Bugfix
        _countDownAudioSrc.enabled = true; // Bugfix
        
        _carScript = GameObject.Find("buggy").GetComponent<CarBehaviour>();
        _carScript.thrustEnabled = false;

        if (gameObject.CompareTag("Finish"))
        {
            _gateStart = GameObject.Find("GateStart").GetComponent<TimingBehaviour>();
        }
        StartCoroutine(GameStart());
    }

    private void StartGameAgain()
    {
        SceneManager.LoadScene("Scene1");
    }
    private void BackToMenu()
    {
        SceneManager.LoadScene("SceneMenu");
    }
    private void LeaderBoardMenu()
    {
        SceneManager.LoadScene("LeaderBoard");
    }


    // GameStart CoRoutine
    IEnumerator GameStart()
    {
        for (_countDown = countMax; _countDown >= 0; _countDown--)
        {
            yield return new WaitForSeconds(1);
            _countDownAudioSrc.pitch = _countDown==0 ? 1.5f : 1.0f;
            _countDownAudioSrc.Play();
            if (timeText)
            {
                print(_countDown);
                timeText.text = "Start in " + _countDown.ToString("0");
            }
            
        }
        _carScript.thrustEnabled = true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (!_isStarted && gameObject.CompareTag("Start"))
            {
                _isStarted = true;
            }
            else
            {
                //Game is Over
                isFinished = true;
            }
            
            if (!_isStarted && gameObject.CompareTag("Finish"))
            {
                _gateStart = GameObject.Find("Gate-Collider-Start").GetComponent<TimingBehaviour>();
                
                _gateStart.OnTriggerEnter(other);
                _gateStart.isFinished = true;
            }

            if (isFinished && gameObject.CompareTag("Start"))
            {
                Debug.Log("FINISH score is :" + _pastTime);
                _prefs.score = _pastTime;
                _prefs.Save();
                SceneManager.LoadScene("LeaderBoard");
            }
        }
    }
    void OnGUI ()
    {
        if (_carScript.thrustEnabled)
        {
            if (_isStarted && !isFinished)
                _pastTime += Time.deltaTime;
            
            if (timeText) timeText.text = _pastTime.ToString("0.0") + " sec.";
            
        }
    }
}
