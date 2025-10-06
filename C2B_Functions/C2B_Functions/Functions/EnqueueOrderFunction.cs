using Azure.Storage.Queues;
using C2B_Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace C2B_Functions.Functions
{
    public class EnqueueOrderFunction
    {
        private readonly ILogger _logger;
        private readonly QueueClient _queueClient;

        public EnqueueOrderFunction(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EnqueueOrderFunction>();
            string connectionString = config["AzureWebJobsStorage"];
            _queueClient = new QueueClient(connectionString, "orders-queue");
            _queueClient.CreateIfNotExists();
        }
        [Function("EnqueueOrder")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var logger = _logger;

            var body = await new StreamReader(req.Body).ReadToEndAsync();

            QueueOrderMessage? orderMessage;
            try
            {
                orderMessage = JsonSerializer.Deserialize<QueueOrderMessage>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync($"Invalid JSON: {ex.Message}");
                return bad;
            }

            if (orderMessage?.Order == null)
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Order data is missing.");
                return bad;
            }

            var json = JsonSerializer.Serialize(orderMessage);
            var base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            logger.LogInformation("Sending base64-encoded message: " + base64Message);
            await _queueClient.SendMessageAsync(base64Message);

            logger.LogInformation($"Order {orderMessage.Order.RowKey} enqueued with {orderMessage.OrderLines.Count} lines.");

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Order queued successfully.");
            return response;
        }
    }
}
