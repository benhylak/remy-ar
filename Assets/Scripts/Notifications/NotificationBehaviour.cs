using static NotificationManager;

public abstract class NotificationBehaviour
{
    private Notification _notificationModel;

    public bool isDismissed;

    public NotificationBehaviour(Notification n)
    {
        this._notificationModel = n;
    }

    public void Dismiss()
    {
        
    }
}
