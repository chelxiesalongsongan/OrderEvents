# OrderEvents

A small event-driven order system using .NET 9 and RabbitMQ. The system consists of a Producer that publishes `OrderPlaced` events and a Consumer that processes and manually acknowledges those events.

## Requirements

* .NET 9 SDK
* Docker Desktop with WSL 2 backend
* Terminal
* Web browser

## 1. Start RabbitMQ

Run:

```powershell
docker run -d --name rabbitmq `
  -p 5672:5672 -p 15672:15672 `
  rabbitmq:3.13-management
```

Confirm that the container is running:

```powershell
docker ps
```

Open the RabbitMQ Management UI:

```text
http://localhost:15672
```

Login using:

```text
Username: guest
Password: guest
```

Under **Queues and Streams**, create a durable queue named:

```text
order.placed
```

The applications also declare the same durable queue in code.

## 2. Build the Solution

From the `OrderEvents` directory, run:

```powershell
dotnet restore
dotnet build
```

## 3. Run the Consumer

Open the first terminal and run:

```powershell
dotnet run --project Consumer
```

The Consumer waits for messages from the `order.placed` queue and manually acknowledges successfully processed events.

Expected output:

```text
Consumer started. Waiting for messages in 'order.placed'...
Press Ctrl+C to stop.
```

## 4. Run the Producer

Open a second terminal and run:

```powershell
dotnet run --project Producer
```

The Producer publishes 10 `OrderPlaced` events.

Expected output:

```text
Published OrderPlaced <OrderId>
```

## 5. Observe the Queue

Open the `order.placed` queue in the RabbitMQ Management UI.

While the Consumer is processing messages, messages may temporarily appear as **Unacked**. After successful processing and acknowledgment, the messages are removed from the queue.

## 6. Manual ACK Stop/Restart Test

Start the Consumer first and then run the Producer.

While messages are still in the **Unacked** state, stop the Consumer using:

```text
Ctrl+C
```

Refresh the RabbitMQ Management UI.

Messages that were not acknowledged are returned to the queue for redelivery. Start the Consumer again:

```powershell
dotnet run --project Consumer
```

The messages are then redelivered and processed.

This demonstrates **at-least-once delivery** using manual acknowledgments.
