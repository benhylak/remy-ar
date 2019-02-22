using System;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public class Notification
    {
        public readonly string message;
        public readonly BurnerBehaviour source;
        public readonly Action Dismiss;
        
        public Notification(string message, BurnerBehaviour source, Action dismiss)
        {
            this.message = message;
            this.source = source;
            this.Dismiss = dismiss;
        }
    }
    
    public delegate void NotificationEventHandler(Notification n);

    private Queue<Notification> _notifications;
    private NotificationBehaviour _currentNotification;
    
    public NotificationManager()
    {
        _notifications = new Queue<Notification>();
    }

    public void AddNotification(Notification n)
    {
        _notifications.Enqueue(n);
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (_currentNotification == null && _notifications.Count > 0)
        {
            var notif = _notifications.Dequeue();
            Debug.LogWarning("Notification: " + notif.message);
            //make a prefab 
        }
        else if (_currentNotification !=null && _currentNotification.isDismissed)
        {
            _currentNotification = null;
        }
    }
}