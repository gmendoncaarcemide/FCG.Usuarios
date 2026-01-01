using FCG.Usuarios.Application.Messaging.Configuration;
using FCG.Usuarios.Application.Messaging.Interfaces;
using FCG.Usuarios.Application.Messaging.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Usuarios.Application.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMQSettings>(options => 
            configuration.GetSection("RabbitMQ").Bind(options));
        
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        return services;
    }
}
