using System.Collections.Generic;
using System.Linq;
using BensToolBox;
using UniRx;
using UnityEngine;

public class BigKahuna: Singleton<BigKahuna>
{
    //TODO: make singleton
    //also make singleton streamer
    
    public DatabaseManager _db;
    public List<BurnerBehaviour> _burnerBehaviours;
    private TimerState _timerState;
    
    public void Start()
    {
        DatabaseManager.Instance.getBurners()
            .ObserveAdd()
            .Subscribe(burnerData => 
                _burnerBehaviours
                    .Find(x => x._position == burnerData.Value.Position)
                    ._model = burnerData.Value);
        
        //bind model and burners
//        _burnerBehaviours
//            .ForEach(x => x._model = 
//                DatabaseManager.Instance.getBurners()
//                    .FirstOrDefault(b => b.Position == x._position));
////        
//        _db.getBurners().ForEach(x => x.IsPotDetected.Subscribe(_ => Debug.Log("PD Changed")));    
//                
//        _db.getBurners().ForEach(x => x.Temperature.Subscribe(_ => Debug.Log("Temp Changed")));
//        
        _timerState = new TimerState.MonitoringState();
    }

    public void Update()
    {
        var resultState = _timerState.Update();

        _timerState = resultState ?? _timerState;
    }
}