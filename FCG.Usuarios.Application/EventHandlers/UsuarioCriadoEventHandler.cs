using FCG.Usuarios.Application.Messaging.Events;
using FCG.Usuarios.Application.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Usuarios.Application.EventHandlers;

public class UsuarioCriadoEventHandler : IEventHandler<UsuarioCriadoEvent>
{
    private readonly ILogger<UsuarioCriadoEventHandler> _logger;
    private readonly IEventBus _eventBus;

    public UsuarioCriadoEventHandler(
        ILogger<UsuarioCriadoEventHandler> logger,
        IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(UsuarioCriadoEvent @event)
    {
        _logger.LogInformation(
            "Processing UsuarioCriadoEvent for User {UsuarioId} - {Email}",
            @event.UsuarioId, @event.Email);

        var notificacao = new NotificacaoEvent
        {
            UsuarioId = @event.UsuarioId,
            Titulo = "Bem-vindo!",
            Mensagem = $"Olá {@event.Nome}, sua conta foi criada com sucesso!",
            Tipo = TipoNotificacao.UsuarioCriado,
            Metadata = new Dictionary<string, string>
            {
                { "Email", @event.Email },
                { "DataCriacao", @event.DataCriacao.ToString("O") }
            }
        };

        await _eventBus.PublishAsync(notificacao);

        _logger.LogInformation(
            "Welcome notification sent for new user {UsuarioId}",
            @event.UsuarioId);
    }
}
