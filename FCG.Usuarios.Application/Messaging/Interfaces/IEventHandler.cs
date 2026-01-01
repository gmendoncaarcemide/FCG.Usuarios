using FCG.Usuarios.Application.Messaging.Events;

namespace FCG.Usuarios.Application.Messaging.Interfaces;

public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event);
}
