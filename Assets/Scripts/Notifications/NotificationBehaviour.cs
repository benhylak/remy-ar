using static NotificationManager;

public abstract class NotificationBehaviour
{
    protected Notification _notificationModel;

    public bool isDismissed;

    public NotificationBehaviour(Notification n)
    {
        this._notificationModel = n;
    }
}
