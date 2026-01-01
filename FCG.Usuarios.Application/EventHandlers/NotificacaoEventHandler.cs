using FCG.Usuarios.Application.Messaging.Events;
using FCG.Usuarios.Application.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Usuarios.Application.EventHandlers;

public class NotificacaoEventHandler : IEventHandler<NotificacaoEvent>
{
    private readonly ILogger<NotificacaoEventHandler> _logger;

    public NotificacaoEventHandler(ILogger<NotificacaoEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(NotificacaoEvent @event)
    {
        _logger.LogInformation(
            "Processing notification for User {UsuarioId}: {Titulo} - {Mensagem}",
            @event.UsuarioId, @event.Titulo, @event.Mensagem);

        _logger.LogInformation(
            "Notification Type: {TipoNotificacao}, Metadata: {Metadata}",
            @event.Tipo, string.Join(", ", @event.Metadata.Select(kv => $"{kv.Key}={kv.Value}")));

        await Task.CompletedTask;
    }
}
