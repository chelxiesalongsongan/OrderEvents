using System.Text.Json;
using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

await channel.BasicQosAsync(
    prefetchSize: 0,
    prefetchCount: 10,
    global: false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    try
    {
        var order = JsonSerializer.Deserialize<OrderPlaced>(
            eventArgs.Body.Span);

        if (order is null)
        {
            throw new InvalidOperationException(
                "Unable to deserialize OrderPlaced event.");
        }

        Console.WriteLine(
    $"Processed OrderPlaced: " +
    $"OrderId={order.OrderId}, " +
    $"StudentId={order.StudentId}, " +
    $"Total={order.Total}, " +
    $"PlacedAtUtc={order.PlacedAtUtc}");

        await Task.Delay(3000);

        await channel.BasicAckAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing message: {ex.Message}");

        await channel.BasicNackAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false,
            requeue: false);
    }
};

await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: false,
    consumer: consumer);

Console.WriteLine(
    $"Consumer started. Waiting for messages in '{queueName}'...");

Console.WriteLine("Press Ctrl+C to stop.");

await Task.Delay(Timeout.Infinite);