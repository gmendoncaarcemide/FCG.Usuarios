using FCG.Usuarios.Domain.Base;

namespace FCG.Usuarios.Domain.Base;

public interface IRepository<T> where T : Entity
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<T>> ObterTodosAsync();
    Task<T> AdicionarAsync(T entity);
    Task<T> AtualizarAsync(T entity);
    Task<bool> ExcluirAsync(Guid id);
} 