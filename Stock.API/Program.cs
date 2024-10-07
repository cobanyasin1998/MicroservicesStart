using MassTransit;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;
using MongoDB.Driver;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, _cfg) =>
    {
        _cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        _cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
    });
});
builder.Services.AddSingleton<Stock.API.Services.MongoDbService>();

#region Harici - MongoDB'ye Seed Data Ekleme
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDBService = scope.ServiceProvider.GetService<MongoDbService>()!;
var collection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 2000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 1000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 3000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 5000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 500 });
}
#endregion
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
