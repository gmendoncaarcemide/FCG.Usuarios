using FCG.Usuarios.Application.Usuarios.ViewModels;
using FCG.Usuarios.Domain.Usuarios.Entities;

namespace FCG.Usuarios.Application.Usuarios.Interfaces;

public interface IUsuarioService
{
    Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request);
    Task<UsuarioResponse?> ObterPorIdAsync(Guid id);
    Task<UsuarioResponse?> ObterPorEmailAsync(string email);
    Task<IEnumerable<UsuarioResponse>> ObterTodosAsync();
    Task<IEnumerable<UsuarioResponse>> ObterPorTipoAsync(TipoUsuario tipoUsuario);
    Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request);
    Task<bool> ExcluirAsync(Guid id);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> AlterarSenhaAsync(Guid id, string senhaAtual, string novaSenha);
    Task<bool> VerificarEmailAsync(string email);
} 