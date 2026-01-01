namespace FCG.Usuarios.Application.Messaging.Events;

public class UsuarioCriadoEvent : IntegrationEvent
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
