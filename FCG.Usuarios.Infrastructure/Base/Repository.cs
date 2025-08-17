using FCG.Usuarios.Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Usuarios.Infrastructure.Base;

public abstract class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly UsuariosDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected Repository(UsuariosDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> ObterTodosAsync()
    {
        return await _dbSet.Where(e => e.Ativo).ToListAsync();
    }

    public virtual async Task<T> AdicionarAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> AtualizarAsync(T entity)
    {
        entity.DataAtualizacao = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> ExcluirAsync(Guid id)
    {
        var entity = await ObterPorIdAsync(id);
        if (entity == null || !entity.Ativo)
            return false;

        entity.Ativo = false;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
} 