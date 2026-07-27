using IntegrationWorker.Service;
using IntegrationWorker.Service.Repositories;
using IntegrationWorker.Service.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IFeedRepository, FeedRepository>();
builder.Services.AddSingleton<FeedProcessor>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();