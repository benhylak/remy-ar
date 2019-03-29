using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BensToolBox;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Linq;
using DG.Tweening;
using UnityEngine.Serialization;

public class BigKahuna: Singleton<BigKahuna>
{
    //TODO: make singleton
    //also make singleton streamer
    
    public DatabaseManager _db;
    public List<BurnerBehaviour> _burnerBehaviours;
    private State _timerState;
    public GameObject Stove;
    public SpeechRecognizer speechRecognizer;
    private bool isSetup = true;
    public IEnumerable<GameObject> _debugObjects;
    private NotificationManager _notificationManager;
    public MeshRenderer raycastVisualizer;

    public SpeechManager alternateSpeechManager;
    
    public void Start()
    {
        DatabaseManager.Instance.getBurners()
            .ObserveAdd()
            .Subscribe(burnerData => 
                _burnerBehaviours
                    .Find(x => x._position == burnerData.Value.Position)
                    ._model = burnerData.Value);
//
//        DatabaseManager
//            .Instance
//            .getBurners()
//            .ObserveCountChanged()
//            .Where(x => x == 4)
//            .Subscribe(_ =>
//                {
//                    Debug.Log("Monitoring...");
//                    _timerState = SetTimerStateMachine.GetInitialState();
//                }
//            );
     
        MLInput.OnControllerButtonDown += (b, button) =>
        {
            if (button == MLInputControllerButton.Bumper)
            {
                ToggleSetup();
            }

            ;
        }; //toggle setup mode.

        MLInput.OnTriggerDown += (_, __) =>
        {
           // alternateSpeechManager.StartSpeechRecognitionFromMicrophone();
         //   _burnerBehaviours.ForEach(x => x.IsLookedAt = true);
        };
        
        MLInput.OnTriggerUp += (_, __) =>
        {
           // _burnerBehaviours.ForEach(x => x.IsLookedAt = false);
        };
        
        _burnerBehaviours.ForEach(b => b.OnBurnerNotification += 
            notif => _notificationManager.AddNotification(notif)
        );

        _notificationManager = GetComponent<NotificationManager>();
        
        _debugObjects = GameObject.FindGameObjectsWithTag("Debug");
    }

    public void Update()
    {
        if (_timerState != null)
        {
            var resultState = _timerState.Update();
            _timerState = resultState ?? _timerState;
        }
    }

    public void ToggleSetup()
    {       
        isSetup = !isSetup;
        
        foreach (var debugObj in _debugObjects)
        {
            debugObj.SetActive(isSetup);
        }

        Stove.GetComponent<ImageTrackerLerper>().TrackingEnabled = false;
        
        if (isSetup)
        {
            raycastVisualizer.material.DOFade(1, 0.3f);
        }
        else raycastVisualizer.material.DOFade(0, 0.3f);
        
    }
}