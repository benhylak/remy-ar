using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{
    public class ControllerGestureObserver : ObservableTriggerBase
    {
        Subject<Unit> onLongTriggerDown;
        public float LONG_TRIGGER_DOWN_TIME = 1f;
        float? raiseTime;
        
        public IObservable<Unit> OnLongTriggerDown()
        {
            return onLongTriggerDown ?? (onLongTriggerDown = new Subject<Unit>());
        }

        private void Start()
        {
            MLInput.OnTriggerDown += (_, __) => { raiseTime = Time.time + LONG_TRIGGER_DOWN_TIME; };
        
            MLInput.OnTriggerUp += (_, __) => { raiseTime = null; };
        }

        void Update()
        {
            if (raiseTime != null && raiseTime <= Time.time)
            {
                onLongTriggerDown?.OnNext(Unit.Default);
                raiseTime = null;
            }
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onLongTriggerDown != null)
            {
                onLongTriggerDown.OnCompleted();
            }
        }
    }
}