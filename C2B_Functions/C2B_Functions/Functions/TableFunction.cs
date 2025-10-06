using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace C2B_Functions.Functions
{
    public class TableFunction
    {
        private readonly ILogger<TableFunction> _logger;
        private readonly string _connectionString;

        public TableFunction(ILogger<TableFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("AzureStorage");
        }

        [Function("AddEntityToTable")]
        public async Task<HttpResponseData> AddEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "table/{tableName}")] HttpRequestData req,
            string tableName)
        {
            var tableClient = new TableClient(_connectionString, tableName);
            await tableClient.CreateIfNotExistsAsync();

            var json = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation("Received JSON: {json}", json);

            var model = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (model == null)
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Invalid entity JSON.");
                return badReq;
            }

            if (!model.ContainsKey("PartitionKey") || string.IsNullOrEmpty(model["PartitionKey"]?.ToString()))
                model["PartitionKey"] = tableName;

            if (!model.ContainsKey("RowKey") || string.IsNullOrEmpty(model["RowKey"]?.ToString()))
                model["RowKey"] = Guid.NewGuid().ToString();

            var entity = new TableEntity(model);

            await tableClient.AddEntityAsync(entity);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Entity added successfully.");
            return response;
        }

        [Function("GetEntitiesFromTable")]
        public async Task<HttpResponseData> GetEntities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "table/{tableName}")] HttpRequestData req,
            string tableName)
        {
            var tableClient = new TableClient(_connectionString, tableName);
            await tableClient.CreateIfNotExistsAsync();

            var entities = tableClient.Query<TableEntity>().ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(entities);
            return response;
        }
    }
}
