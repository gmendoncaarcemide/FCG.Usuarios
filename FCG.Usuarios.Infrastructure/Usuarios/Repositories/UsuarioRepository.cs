using FCG.Usuarios.Domain.Usuarios.Entities;
using FCG.Usuarios.Domain.Usuarios.Interfaces;
using FCG.Usuarios.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Usuarios.Infrastructure.Usuarios.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(UsuariosDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.Ativo);
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email && u.Ativo);
    }

    public async Task<IEnumerable<Usuario>> ObterPorTipoAsync(TipoUsuario tipoUsuario)
    {
        return await _dbSet.Where(u => u.TipoUsuario == tipoUsuario && u.Ativo).ToListAsync();
    }
} 