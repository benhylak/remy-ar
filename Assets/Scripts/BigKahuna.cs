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
using UnityEngine.Serialization;

public class BigKahuna: Singleton<BigKahuna>
{
    //TODO: make singleton
    //also make singleton streamer
    
    public DatabaseManager _db;
    public List<BurnerBehaviour> _burnerBehaviours;
    private TimerState _timerState;
    public MLImageTrackerBehavior _stoveTracker;
    public GameObject burners;
    public SpeechRecognizer speechRecognizer;
    private bool isSetup = true;
    public IEnumerable<GameObject> _debugObjects;
    private NotificationManager _notificationManager;
    
    public void Start()
    {
        DatabaseManager.Instance.getBurners()
            .ObserveAdd()
            .Subscribe(burnerData => 
                _burnerBehaviours
                    .Find(x => x._position == burnerData.Value.Position)
                    ._model = burnerData.Value);

        DatabaseManager
            .Instance
            .getBurners()
            .ObserveCountChanged()
            .Where(x => x == 4)
            .Subscribe(_ =>
                {
                    Debug.Log("Monitoring...");
                    _timerState = new TimerState.MonitoringState();
                }
            );
     
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
            _burnerBehaviours.ForEach(x => x.IsLookedAt = true);
        };
        
        MLInput.OnTriggerUp += (_, __) =>
        {
            _burnerBehaviours.ForEach(x => x.IsLookedAt = false);
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

        if (isSetup && _stoveTracker.IsTracking && _stoveTracker.TrackingStatus == MLImageTargetTrackingStatus.Tracked)
        {
            burners.transform.position = Vector3.Lerp(burners.transform.position, _stoveTracker.transform.position, Time.deltaTime);
            burners.transform.rotation = Quaternion.Slerp(burners.transform.rotation, _stoveTracker.transform.rotation, Time.deltaTime);
        }    
    }

    public void ToggleSetup()
    {       
        isSetup = !isSetup;
        
        foreach (var debugObj in _debugObjects)
        {
            debugObj.SetActive(isSetup);
            
            #if !UNITY_EDITOR
                if (isSetup)
                {
                    MLImageTracker.Enable();
                }
                else MLImageTracker.Disable();
            #endif         
        }
    }
}