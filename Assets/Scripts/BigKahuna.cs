using System.Collections.Generic;
using System.Linq;
using BensToolBox;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class BigKahuna: Singleton<BigKahuna>
{
    //TODO: make singleton
    //also make singleton streamer
    
    public DatabaseManager _db;
    public List<BurnerBehaviour> _burnerBehaviours;
    private TimerState _timerState;
    public MLImageTrackerBehavior _stoveTracker;
    public GameObject burners;
    public bool setup = true;
    
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
                Debug.Log("Button Down!");
                setup = !setup;
            }
        }; //toggle setup mode.
//        
   
    }

    public void Update()
    {
        if (_timerState != null)
        {
            var resultState = _timerState.Update();
            _timerState = resultState ?? _timerState;
        }

        if (setup && _stoveTracker.IsTracking && _stoveTracker.TrackingStatus == MLImageTargetTrackingStatus.Tracked)
        {
            burners.transform.position = Vector3.Lerp(burners.transform.position, _stoveTracker.transform.position, Time.deltaTime);
            burners.transform.rotation = Quaternion.Slerp(burners.transform.rotation, _stoveTracker.transform.rotation, Time.deltaTime);
        }
    }
}