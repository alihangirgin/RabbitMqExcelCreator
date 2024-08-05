using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using RabbitMQ.Client;

namespace RabbitMqExcelCreator.Services
{
    public class RabbitMqClientService : IRabbitMqClientService, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMqClientService> _logger;
        private readonly string _exchangeName = "excelExchange";
        private readonly string _routingKey = "route-excel";
        private readonly string _queueName = "queue-excel";

        public RabbitMqClientService(IConnectionFactory connectionFactory, ILogger<RabbitMqClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            if (_channel is { IsOpen: true })
                return _channel;

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, true, false);
            _channel.QueueDeclare(_queueName, true, false, false);
            _channel.QueueBind(_queueName, _exchangeName, _routingKey);

            _logger.LogInformation("RabbitMq Connected");

            return _channel;
        }

        public void PublishMessage<T>(T messageObject)
        {
            if (_channel is not { IsOpen: true })
                Connect();

            var jsonMessage = JsonSerializer.Serialize(messageObject);
            var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(_exchangeName, _routingKey, properties, messageBytes);

            _logger.LogInformation("RabbitMq Message Published: {jsonMessage}", jsonMessage);
        }

        public void Dispose()
        {
            if (_channel is { IsOpen: true })
            {
                _channel.Close();
                _channel.Dispose();
            }

            if (_connection is { IsOpen: true })
            {
                _connection.Close();
                _connection.Dispose();
            }

            _logger.LogInformation("RabbitMq Disposed");
        }
    }
}
