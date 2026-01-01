namespace FCG.Usuarios.Application.Messaging.Events;

public abstract class IntegrationEvent
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string EventType { get; private set; }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }
}
