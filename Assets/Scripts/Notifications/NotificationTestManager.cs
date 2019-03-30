using System;
using System.Collections.Generic;
using BensToolBox.AR.Scripts;
using DG.Tweening;
using MagicLeap;
using Newtonsoft.Json;
using SimpleFirebaseUnity;
using SimpleFirebaseUnity.MiniJSON;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class NotificationTestManager : MonoBehaviour
{
    public string EXPERIMENT_NAME = "AlphaTest";
    
    public enum NotificationType
    {
        Physical,
        HeadsUp,
        Hugger
    }
    
    public class Experiment
    {
        public DateTime experiment_start = DateTime.Now;
        public float experiment_end;
        public Dictionary<NotificationType, List<Gaze>> gazes = new Dictionary<NotificationType, List<Gaze>>();

        public delegate void ExperimentUpdateHandler(Experiment e);

        public int notificationType = -1;

        [JsonIgnore] public Firebase Endpoint;      
        [JsonIgnore] public ExperimentUpdateHandler OnExperimentUpdated;
        
        public void AddGaze(NotificationType notifType, Gaze g)
        {
            if (!gazes.ContainsKey(notifType))
            {
                gazes[notifType] = new List<Gaze>();
            }
            
            gazes[notifType].Add(g);

            Endpoint.SetValue(JsonConvert.SerializeObject(this), true);

           // OnExperimentUpdated(this);
        }

        public void EndExperiment()
        {
            experiment_end = Time.time;
            Endpoint.SetValue(JsonConvert.SerializeObject(this), true);
        }
    }

    public class Gaze
    {
        public float duration;
        public DateTime gaze_start;
        
        public Gaze(float duration)
        {
            this.duration = duration;
            gaze_start = DateTime.Now - TimeSpan.FromSeconds(duration);
        }
    }

    
    [SerializeField]
    private MeshRenderer eyeRaycastVisualizer;

    [SerializeField]
    private HeadsUpNotification headsUpNotification;

    [SerializeField]
    private PhysicalNotification physicalNotification;

    private float triggerDownStartTime = -1;
    private float startExperimentTriggerDownThreshold = 0.5f;

    private Firebase _firebase;

    private Experiment _experiment;

    private bool _ready = true;
    
    
    public void Start()
    {
                  
        var gestureObserver = gameObject.AddComponent<ControllerGestureObserver>();

        gestureObserver.OnLongTriggerDown().Subscribe(_ =>
        {
            if (physicalNotification.GetComponent<MoveWithControllerBehaviour>().isMoving)
            {
                return;
            }
            
            if (_experiment == null)
            {
                headsUpNotification.Hide();
                physicalNotification.Hide()
                    .OnComplete(StartExperiment);
           
                Debug.LogWarning("Starting Experiment");
            }
            else
            {
                _experiment.EndExperiment();
                _experiment = null;
                
                headsUpNotification.HideToShow();
                physicalNotification.HideToShow();
                
                Debug.LogWarning("Ended Experiment");               
            }
        });

        //var seq = DOTween.Sequence();  
        
        MLInput.OnControllerButtonDown += (b, button) =>
        {
            if (button == MLInputControllerButton.Bumper)
            {
                if (_experiment == null)
                {
                    physicalNotification.Launch();
                    headsUpNotification.Launch();
                }
                else if(_experiment.notificationType > -1)
                {
                    switch ((NotificationType)_experiment.notificationType)
                    {
                        case NotificationType.Physical:
                            physicalNotification.Launch(2000);
                            break;
                        
                        case NotificationType.HeadsUp:
                            headsUpNotification.Launch(2000);
                            break;
                    }
                }
            }
            else if (button == MLInputControllerButton.HomeTap)
            {
                eyeRaycastVisualizer.enabled = !eyeRaycastVisualizer.enabled;
            }
        };

        headsUpNotification.gameObject.GetComponent<GazeReceiver>().OnGazeExit.Subscribe(duration =>
        {
            if(_experiment!=null)
                _experiment.AddGaze(NotificationType.HeadsUp, new Gaze(duration));
                Debug.Log(JsonConvert.SerializeObject(_experiment));
                  
            //Debug.LogError("Gaze Exit: Lasted " + duration + " seconds.");
        });
        
        _firebase = Firebase
            .CreateNew(Secrets.FIREBASE_URL, Secrets.FIREBASE_CREDENTIAL)
            .Child("Experiments")
            .Child(EXPERIMENT_NAME);
        
        MLInput.OnTriggerDown += (b, f) => { headsUpNotification.Launch(); };
    }

    public void OnExperimentPushed(Firebase f, DataSnapshot s)
    {
        _experiment.Endpoint = f;
    }

    public async void StartExperiment()
    {
        _experiment = new Experiment();

        _firebase.OnPushSuccess = (f, s) =>
        {
            var id = s.GetValueForKey<string>(s.FirstKey);

            _experiment.Endpoint = f.Child(id);
            _firebase.OnPushSuccess = null;
        };
       
        _firebase.Push(JsonConvert.SerializeObject(_experiment), true);
    }

    public void ChangeNotificationType(NotificationType notifType)
    {
        Debug.LogWarning("Changing notificaiton type to: " + notifType);
        switch (notifType)
        {
            case NotificationType.HeadsUp:
                physicalNotification.Hide()
                    .OnComplete(() => headsUpNotification.Launch());
                
                break;
                
            case NotificationType.Physical:
                headsUpNotification.Hide()
                    .OnComplete(() => physicalNotification.Launch());
                
                break;
        } 
    }

    private void Update()
    {
       FetchNotificationType();
    }

    private async void FetchNotificationType()
    {
        if (_ready && _experiment != null && _experiment.Endpoint != null)
        {
            _ready = false; 
            
            DataSnapshot s = await _experiment.Endpoint.Child("notificationType").GetValue();

            int value = -1;
            
            try
            {
                value = Convert.ToInt32(s.RawValue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _ready = true;
                return;
            }
                       
            if (value > 0 && Enum.IsDefined(typeof(NotificationType), value))
            {
                if (_experiment != null && _experiment.notificationType != value)
                {
                    _experiment.notificationType = value;
                    ChangeNotificationType((NotificationType)value);
                }
            }

            _ready = true;
        }
    }
}