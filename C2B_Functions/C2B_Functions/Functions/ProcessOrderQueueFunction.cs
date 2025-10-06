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
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            _logger.LogInformation("Raw queue message: " + queueMessage);

            QueueOrderMessage? orderMessage;
            try
            {
                orderMessage = JsonSerializer.Deserialize<QueueOrderMessage>(queueMessage, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize queue message.");
                return;
            }

            if (orderMessage?.Order == null)
            {
                _logger.LogError("Order is null in queue message.");
                return;
            }

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
