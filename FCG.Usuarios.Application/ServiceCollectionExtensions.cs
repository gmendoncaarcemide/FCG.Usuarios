using FCG.Usuarios.Application.Usuarios.Interfaces;
using FCG.Usuarios.Application.Usuarios.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Usuarios.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsuariosService(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        
        return services;
    }
}
