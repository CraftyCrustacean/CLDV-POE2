using System.Net.Http.Json;
using C2B_POE1.Models;

namespace C2B_POE1.Data
{
    public class OrdersFunctionService
    {
        private readonly HttpClient _httpClient;

        public OrdersFunctionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task EnqueueOrderAsync(Order order, List<OrderLine> lines)
        {
            var queueMessage = new QueueOrderMessage
            {
                Order = new QueueOrder
                {
                    PartitionKey = order.PartitionKey,
                    RowKey = order.RowKey,
                    Fufilled = order.Fufilled
                },
                OrderLines = lines.Select(l => new QueueOrderLine
                {
                    PartitionKey = $"Order_{order.RowKey}",
                    RowKey = l.RowKey,
                    ProductRowKey = l.ProductRowKey,
                    Quantity = l.Quantity
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/EnqueueOrder", queueMessage);

            response.EnsureSuccessStatusCode();
        }
    }
}
