using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqExcelCreator.FileWorkerService.Models;
using RabbitMqExcelCreator.Models;
using RabbitMqExcelCreator.Services;

namespace RabbitMqExcelCreator.FileWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRabbitMqClientService _rabbitMqClientService;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, IRabbitMqClientService rabbitMqClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMqClientService = rabbitMqClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMqClientService.Connect();
            _channel.BasicQos(0,1,false);
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            _logger.LogInformation("Waiting for messages.");

            consumer.Received += async (sender, @event) =>
            {
                await Consumer_Received(sender, @event);
            };

            _channel.BasicConsume(_rabbitMqClientService.GetQueueName(), false, consumer);
        }

        private async Task Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Message received:{message}");
            var excel = JsonSerializer.Deserialize<CreateExcelMessage>(message);

            using var memoryStream = new MemoryStream();
            var workBook = new XLWorkbook();
            var dataSet = new DataSet();
            dataSet.Tables.Add(GetTable("Product"));
            workBook.Worksheets.Add(dataSet);
            workBook.SaveAs(memoryStream);
            memoryStream.Position = 0;

            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(memoryStream.ToArray());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            form.Add(fileContent, "iFormFile", $"{Guid.NewGuid().ToString()}.xlsx");
            form.Add(new StringContent(excel?.UserFileId.ToString() ?? string.Empty), "userFileId");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync("https://localhost:7119/api/file/upload", form);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"File Id:{excel?.UserFileId} was sent successfully");
                _channel.BasicAck(e.DeliveryTag, false);
            }

        }

        private DataTable GetTable(string tableName)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
            var products = context.Products.ToList();
            var table = new DataTable(tableName = tableName);

            table.Columns.Add("ProductId", typeof(int)); 
            table.Columns.Add("Name", typeof(string)); 
            table.Columns.Add("ProductNumber", typeof(string)); 
            table.Columns.Add("Color", typeof(string)); 

            foreach (var product in products)
            {
                table.Rows.Add(product.ProductId, product.Name, product.ProductNumber, product.Color);
            }
            return table;
        }

    }
}
