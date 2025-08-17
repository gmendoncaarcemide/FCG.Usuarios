using System.ComponentModel.DataAnnotations;
using FCG.Usuarios.Domain.Usuarios.Entities;

namespace FCG.Usuarios.Application.Usuarios.ViewModels;

public class CriarUsuarioRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string Senha { get; set; } = string.Empty;

    public TipoUsuario TipoUsuario { get; set; } = TipoUsuario.Usuario;
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Endereco { get; set; }
}

public class AtualizarUsuarioRequest
{
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string? Nome { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string? Senha { get; set; }

    public TipoUsuario? TipoUsuario { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Endereco { get; set; }
}

public class UsuarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Endereco { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UsuarioResponse Usuario { get; set; } = new();
    public DateTime DataExpiracao { get; set; }
}

public class AlterarSenhaRequest
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Nova senha deve ter entre 6 e 100 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;
}