using RabbitMQ.Client;

namespace RabbitMqExcelCreator.Services
{
    public interface IRabbitMqClientService
    {
        IModel Connect();
        string GetQueueName();
    }
}
