using FCG.Usuarios.Application.Messaging.Configuration;
using FCG.Usuarios.Application.Messaging.Events;
using FCG.Usuarios.Application.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FCG.Usuarios.Application.Messaging.RabbitMQ;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly Dictionary<string, Type> _eventTypes = new();
    private readonly Dictionary<string, Type> _handlerTypes = new();
    private const string ExchangeName = "fcg_events";

    public RabbitMQEventBus(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null) where TEvent : IntegrationEvent
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("RabbitMQ channel is not open. Attempting to reconnect...");
            InitializeRabbitMQ();
        }

        var eventName = @event.GetType().Name;
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = eventName;
        properties.MessageId = @event.Id.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var finalRoutingKey = routingKey ?? eventName;

        for (int retry = 0; retry <= _settings.RetryCount; retry++)
        {
            try
            {
                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: finalRoutingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Published event {EventName} with ID {EventId} to routing key {RoutingKey}",
                    eventName, @event.Id, finalRoutingKey);

                await Task.CompletedTask;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error publishing event {EventName} (attempt {Retry}/{MaxRetries})",
                    eventName, retry + 1, _settings.RetryCount + 1);

                if (retry == _settings.RetryCount)
                    throw;

                await Task.Delay(_settings.RetryDelayMilliseconds);
            }
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var queueName = $"{eventName}_queue";

        _eventTypes[eventName] = typeof(TEvent);
        _handlerTypes[eventName] = typeof(THandler);

        if (_channel == null || !_channel.IsOpen)
        {
            InitializeRabbitMQ();
        }

        _channel!.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: queueName,
            exchange: ExchangeName,
            routingKey: eventName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var eventName = ea.RoutingKey;
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);
                _channel.BasicAck(ea.DeliveryTag, false);
                
                _logger.LogInformation(
                    "Successfully processed event {EventName} with delivery tag {DeliveryTag}",
                    eventName, ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing event {EventName}. Message: {Message}",
                    eventName, message);

                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Subscribed to event {EventName} with handler {HandlerName}",
            eventName, typeof(THandler).Name);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (!_eventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("No event type registered for {EventName}", eventName);
            return;
        }

        if (!_handlerTypes.TryGetValue(eventName, out var handlerType))
        {
            _logger.LogWarning("No handler registered for {EventName}", eventName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var @event = JsonConvert.DeserializeObject(message, eventType);
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler == null)
        {
            _logger.LogError("Could not resolve handler {HandlerType}", handlerType.Name);
            return;
        }

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod != null)
        {
            await (Task)handleMethod.Invoke(handler, new[] { @event })!;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
