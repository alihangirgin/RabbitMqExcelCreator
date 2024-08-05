using RabbitMQ.Client;

namespace RabbitMqExcelCreator.Services
{
    public interface IRabbitMqClientService
    {
        IModel Connect();
        void PublishMessage<T>(T messageObject);
    }
}
