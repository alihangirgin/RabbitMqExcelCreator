using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMqExcelCreator.FileWorkerService;
using RabbitMqExcelCreator.FileWorkerService.Models;
using RabbitMqExcelCreator.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IConnectionFactory>(x => new ConnectionFactory
{
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMq") ?? string.Empty)
});
builder.Services.AddSingleton<IRabbitMqClientService, RabbitMqClientService>();

var connectionString = builder.Configuration.GetConnectionString("AdventureWorksDb");
builder.Services.AddDbContext<AdventureWorks2019Context>(options => options.UseSqlServer(connectionString));

var host = builder.Build();
host.Run();
