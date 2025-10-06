using Azure.Storage.Queues;
using System.Text.Json;

public class QueueService
{
    private readonly QueueClient _queueClient;

    public QueueService(string connectionString, string queueName)
    {
        _queueClient = new QueueClient(connectionString, queueName);
        _queueClient.CreateIfNotExists();
    }


    public async Task EnqueueMessageAsync(object message)
    {

        string msg = JsonSerializer.Serialize(message);

        if (string.IsNullOrWhiteSpace(msg))
            throw new ArgumentException("Message cannot be empty");

        await _queueClient.SendMessageAsync(
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(msg))
        );
    }

    public class OrdersQueueService : QueueService
    {
        public OrdersQueueService(string connectionString)
            : base(connectionString, "orders") { }
    }

    public class ImagesQueueService : QueueService
    {
        public ImagesQueueService(string connectionString)
            : base(connectionString, "images") { }
    }
}