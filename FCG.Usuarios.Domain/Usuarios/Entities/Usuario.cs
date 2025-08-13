using FCG.Usuarios.Domain.Base;

namespace FCG.Usuarios.Domain.Usuarios.Entities;

public class Usuario : Entity
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Senha { get; set; }
    public TipoUsuario TipoUsuario { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Endereco { get; set; }
}

public enum TipoUsuario
{
    Administrador = 1,
    Usuario = 2,
    Moderador = 3
} 