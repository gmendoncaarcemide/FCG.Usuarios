using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Usuarios.Infrastructure
{
    public class UsuariosDbContextFactory : IDesignTimeDbContextFactory<UsuariosDbContext>
    {
        public UsuariosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsuariosDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FCG_Usuarios;Trusted_Connection=true;");

            return new UsuariosDbContext(optionsBuilder.Options);
        }
    }
}
