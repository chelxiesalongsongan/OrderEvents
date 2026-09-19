using System.Text.Json;
using Contracts;
using RabbitMQ.Client;

const string queueName = "order.placed";

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: queueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

for (int i = 1; i <= 10; i++)
{
    var order = new OrderPlaced(
        OrderId: Guid.NewGuid(),
        StudentId: $"COLLEGELAB-10-{i:00}",
        Total: 500m + (i * 100m),
        PlacedAtUtc: DateTime.UtcNow);

    var body = JsonSerializer.SerializeToUtf8Bytes(order);

    var properties = new BasicProperties
    {
        Persistent = true,
        MessageId = order.OrderId.ToString()
    };

    await channel.BasicPublishAsync(
        exchange: "",
        routingKey: queueName,
        mandatory: false,
        basicProperties: properties,
        body: body);

    Console.WriteLine($"Published OrderPlaced {order.OrderId}");

    await Task.Delay(200);
}