namespace FCG.Usuarios.Application.Messaging.Events;

public class NotificacaoEvent : IntegrationEvent
{
    public Guid UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public TipoNotificacao Tipo { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum TipoNotificacao
{
    UsuarioCriado = 1,
    PagamentoIniciado = 2,
    PagamentoAprovado = 3,
    PagamentoRecusado = 4,
    CompraRealizada = 5
}
