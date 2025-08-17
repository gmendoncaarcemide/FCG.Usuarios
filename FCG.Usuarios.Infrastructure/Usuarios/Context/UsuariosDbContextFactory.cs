using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Usuarios.Infrastructure
{
    public class UsuariosDbContextFactory : IDesignTimeDbContextFactory<UsuariosDbContext>
    {
        public UsuariosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsuariosDbContext>();
            optionsBuilder.UseNpgsql("Host=db.elcvczlnnzbgcpsbowkg.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Fiap@1234");

            return new UsuariosDbContext(optionsBuilder.Options);
        }
    }
}
