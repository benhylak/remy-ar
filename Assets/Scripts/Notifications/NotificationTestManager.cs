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
using UnityEngine.Serialization;
using UnityEngine.XR.MagicLeap;

public class NotificationTestManager : MonoBehaviour
{
    public string EXPERIMENT_NAME = "NotifTest1";

    public string TIMER_DONE = "TIMER_DONE";
    public string BURNER_ON = "BURNER_ON";
    
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
        public string currentMessageType;
        
        [JsonIgnore] public Firebase Endpoint;      
        [JsonIgnore] public ExperimentUpdateHandler OnExperimentUpdated;
        
        public void AddGaze(NotificationType notifType, Gaze g)
        {
            if (!gazes.ContainsKey(notifType))
            {
                gazes[notifType] = new List<Gaze>();
            }

            g.messageType = currentMessageType;
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
        public string messageType;
        
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

    [FormerlySerializedAs("physicalNotification")] [SerializeField]
    public PhysicalNotification timerDonePhysicalNotif;
    public PhysicalNotification burnerOnPhysicalNotif;


    private float triggerDownStartTime = -1;
    private float startExperimentTriggerDownThreshold = 0.5f;

    public NotificationBehaviour _lastNotification;
    public string _lastMessageType;
    public NotificationType _lastNotifType;
    
    private Firebase _firebase;

    private Experiment _experiment;

    private bool _ready = true;

    private bool _hidden = false;
    
    
    public void Start()
    {                
        _firebase = Firebase
            .CreateNew(Secrets.FIREBASE_URL, Secrets.FIREBASE_CREDENTIAL)
            .Child("Experiments")
            .Child(EXPERIMENT_NAME);
        
        var gestureObserver = gameObject.AddComponent<ControllerGestureObserver>();

        gestureObserver.OnLongTriggerDown().Subscribe(_ =>
        {
            if (timerDonePhysicalNotif.GetComponent<MoveWithControllerBehaviour>().isMoving)
            {
                return;
            }
            
            if (_experiment == null)
            {
                headsUpNotification.Hide();
                timerDonePhysicalNotif.Hide()
                    .OnComplete(StartExperiment);
           
                Debug.LogWarning("Starting Experiment");
            }
            else
            {
                _experiment.EndExperiment();
                _experiment = null;
                
                headsUpNotification.HideToShow();
                timerDonePhysicalNotif.HideToShow();
                
                Debug.LogWarning("Ended Experiment");               
            }
        });

        _lastNotification = timerDonePhysicalNotif;
        burnerOnPhysicalNotif.Hide();

        //var seq = DOTween.Sequence();  
        
        MLInput.OnControllerButtonDown += (b, button) =>
        {
            if (button == MLInputControllerButton.Bumper)
            {
                if (_hidden)
                {
                    _hidden = false;
                    burnerOnPhysicalNotif.Launch();
                    timerDonePhysicalNotif.Launch();
                    headsUpNotification.Launch();
                }
                else
                {
                    _hidden = true;
                    timerDonePhysicalNotif.Hide();
                    burnerOnPhysicalNotif.Hide();
                    headsUpNotification.Hide();
                }
            }
            else if (button == MLInputControllerButton.HomeTap)
            {
                eyeRaycastVisualizer.enabled = !eyeRaycastVisualizer.enabled;
            }
        };

        headsUpNotification.gameObject.GetComponent<GazeReceiver>().OnGazeExit.Subscribe(duration =>
        {
            _experiment?.AddGaze(NotificationType.HeadsUp, new Gaze(duration));
         //   Debug.Log(JsonConvert.SerializeObject(_experiment));                  
            //Debug.LogError("Gaze Exit: Lasted " + duration + " seconds.");
        });
        

        //MLInput.OnTriggerDown += (b, f) => { headsUpNotification.Launch(); };
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

    public void ChangeNotificationType(NotificationType notifType, string messageType)
    {
        Debug.LogWarning($"Changing notification type to: {notifType} {messageType}");
        
        switch (notifType)
        {
            case NotificationType.HeadsUp:
                headsUpNotification.SetText(messageType == BURNER_ON ? "Burner Left On" : "Timer Done");
                Debug.Log("Heads up");
                
                _lastNotification.Hide()
                    .OnComplete(() => headsUpNotification.Launch());

                _lastNotification = headsUpNotification;
                
                break;
                
            case NotificationType.Physical:
                _lastNotification.Hide()
                    .OnComplete(() =>
                    {
                        Debug.Log("Physical ;)");   
                        
                        var chosenNotif = messageType == BURNER_ON ? burnerOnPhysicalNotif : timerDonePhysicalNotif;
                        chosenNotif.Launch();

                        var sound = chosenNotif._notifSound;
                        
                        if (sound != null)
                        {
                            GetComponent<AudioSource>().clip = sound;
                            GetComponent<AudioSource>().PlayDelayed(0.3f);
                        }
                        
                        _lastNotification = chosenNotif;
                    });
                
                break;
        }

        _lastNotifType = notifType;
        _lastMessageType = messageType;
    }

    private void Update()
    {
       FetchNotificationType();
    }

    private async void FetchNotificationType()
    {
        if (_ready)
        {
            _ready = false; 
            
            DataSnapshot notifType = await _firebase.Child("notifType").GetValue();
                
            int value = -1;
            
            try
            {
                value = Convert.ToInt32(notifType.RawValue);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Console.WriteLine(e);
                _ready = true;
                return;
            }

            if (value < 0)
            {
                burnerOnPhysicalNotif.Hide();
                headsUpNotification.Hide();
                timerDonePhysicalNotif.Hide();
            }
            else if (Enum.IsDefined(typeof(NotificationType), value))
            {
                string messageType = (string)(await _firebase.Child("messageType").GetValue()).RawValue;    
                
                if (_experiment != null)
                {
                    _experiment.notificationType = value;
                    _experiment.currentMessageType = messageType;
                }

                if (_lastNotifType != (NotificationType)value || _lastMessageType != messageType)
                {
                    Debug.Log("Change notification!");   
                    ChangeNotificationType((NotificationType)value, messageType);
                }
            }

            _ready = true;
        }
    }
}