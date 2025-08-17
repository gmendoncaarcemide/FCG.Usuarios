using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FCG.Usuarios.Domain.Usuarios.Interfaces;
using FCG.Usuarios.Infrastructure.Usuarios.Repositories;

namespace FCG.Usuarios.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsuariosDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<UsuariosDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}
