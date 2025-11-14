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

            var model = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (model == null)
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Invalid entity JSON.");
                return badReq;
            }

            var caseInsensitiveModel = new Dictionary<string, object>(model, StringComparer.OrdinalIgnoreCase);

            if (!caseInsensitiveModel.ContainsKey("PartitionKey") ||
                string.IsNullOrEmpty(caseInsensitiveModel["PartitionKey"]?.ToString()))
            {
                _logger.LogWarning("No PartitionKey provided, using table name as default");
                caseInsensitiveModel["PartitionKey"] = tableName;
            }

            if (!caseInsensitiveModel.ContainsKey("RowKey") ||
                string.IsNullOrEmpty(caseInsensitiveModel["RowKey"]?.ToString()))
            {
                caseInsensitiveModel["RowKey"] = Guid.NewGuid().ToString();
            }

            var entity = new TableEntity();
            foreach (var kvp in caseInsensitiveModel)
            {
                if (kvp.Key.Equals("PartitionKey", StringComparison.OrdinalIgnoreCase))
                {
                    entity.PartitionKey = kvp.Value?.ToString() ?? "";
                }
                else if (kvp.Key.Equals("RowKey", StringComparison.OrdinalIgnoreCase))
                {
                    entity.RowKey = kvp.Value?.ToString() ?? "";
                }
                else
                {
                    entity[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation($"Upserting entity - PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");

            await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Entity added/updated successfully.");
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

        [Function("GetEntityFromTable")]
        public async Task<HttpResponseData> GetEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "table/{tableName}/{rowKey}")] HttpRequestData req,
            string tableName,
            string rowKey)
        {
            var response = req.CreateResponse();

            try
            {
                var tableClient = new TableClient(_connectionString, tableName);
                await tableClient.CreateIfNotExistsAsync();

                var entity = tableClient.Query<TableEntity>(e => e.RowKey == rowKey).FirstOrDefault();

                if (entity == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    await response.WriteStringAsync($"Could not find entity with RowKey '{rowKey}' in '{tableName}'.");
                    return response;
                }

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(entity);
                _logger.LogInformation($"Successfully retrieved entity {rowKey} from {tableName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving entity {rowKey} from {tableName}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error retrieving entity: {ex.Message}");
            }

            return response;
        }

        [Function("DeleteEntityFromTable")]
        public async Task<HttpResponseData> DeleteEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "table/{tableName}/{partitionKey}/{rowKey}")] HttpRequestData req,
            string tableName,
            string partitionKey,
            string rowKey)
        {
            var response = req.CreateResponse();

            try
            {
                var tableClient = new TableClient(_connectionString, tableName);
                await tableClient.CreateIfNotExistsAsync();

                await tableClient.DeleteEntityAsync(partitionKey, rowKey);

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync($"Deleted entity with PartitionKey '{partitionKey}' and RowKey '{rowKey}' from '{tableName}'.");

                _logger.LogInformation($"Successfully deleted entity {partitionKey}/{rowKey} from {tableName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting entity {partitionKey}/{rowKey} from {tableName}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error deleting entity: {ex.Message}");
            }

            return response;
        }
    }
}