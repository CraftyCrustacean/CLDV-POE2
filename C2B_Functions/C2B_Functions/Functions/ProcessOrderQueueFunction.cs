using Azure.Data.Tables;
using C2B_Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace C2B_Functions.Functions
{
    public class ProcessOrderQueueFunction
    {
        private readonly ILogger _logger;
        private readonly TableClient _ordersTable;
        private readonly TableClient _orderLinesTable;

        public ProcessOrderQueueFunction(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessOrderQueueFunction>();
            string connectionString = config["AzureWebJobsStorage"];

            _ordersTable = new TableClient(connectionString, "Orders");
            _orderLinesTable = new TableClient(connectionString, "OrderLines");

            _ordersTable.CreateIfNotExists();
            _orderLinesTable.CreateIfNotExists();
        }

        [Function("ProcessOrderQueue")]
        public async Task Run(
            [QueueTrigger("orders-queue", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            _logger.LogInformation("Raw queue message: " + queueMessage);

            // Decode from base64 first
            string decodedJson;
            try
            {
                var bytes = Convert.FromBase64String(queueMessage);
                decodedJson = Encoding.UTF8.GetString(bytes);
                _logger.LogInformation("Decoded JSON: " + decodedJson);
            }
            catch
            {
                // If it's not base64, use as-is
                decodedJson = queueMessage;
                _logger.LogInformation("Not base64, using as-is");
            }

            // Deserialize the order message
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            QueueOrderMessage? orderMessage;

            try
            {
                orderMessage = JsonSerializer.Deserialize<QueueOrderMessage>(decodedJson, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize: " + decodedJson);
                return;
            }

            if (orderMessage?.Order == null)
            {
                _logger.LogError("Order is null");
                return;
            }

            // Add to tables
            await _ordersTable.AddEntityAsync(orderMessage.Order);

            if (orderMessage.OrderLines != null)
            {
                foreach (var line in orderMessage.OrderLines)
                {
                    await _orderLinesTable.AddEntityAsync(line);
                }
            }

            _logger.LogInformation(
                "Order {orderRowKey} and {count} lines written to Azure Tables.",
                orderMessage.Order.RowKey, orderMessage.OrderLines?.Count ?? 0);
        }
    }
}