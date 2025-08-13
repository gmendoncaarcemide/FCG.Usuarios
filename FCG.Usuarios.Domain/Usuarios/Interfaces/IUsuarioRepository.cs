using FCG.Usuarios.Domain.Usuarios.Entities;

namespace FCG.Usuarios.Domain.Usuarios.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> ObterTodosAsync();
    Task<Usuario> AdicionarAsync(Usuario usuario);
    Task<Usuario> AtualizarAsync(Usuario usuario);
    Task<bool> ExcluirAsync(Guid id);
    Task<IEnumerable<Usuario>> ObterPorTipoAsync(TipoUsuario tipoUsuario);
} 