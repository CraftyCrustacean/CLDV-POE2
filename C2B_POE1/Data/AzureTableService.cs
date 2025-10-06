using System.Net.Http.Json;
using Azure.Data.Tables;

namespace C2B_POE1.Data
{
    public class AzureTableService<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _tableName;
        private readonly string _baseUrl;

        public AzureTableService(HttpClient httpClient, string tableName, string baseUrl)
        {
            _httpClient = httpClient;
            _tableName = tableName;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<T>>($"{_baseUrl}/{_tableName}") ?? new List<T>();
        }

        public async Task<T?> GetAsync(string rowKey)
        {
            return await _httpClient.GetFromJsonAsync<T>($"{_baseUrl}/{_tableName}/{rowKey}");
        }

        public async Task AddAsync(T entity)
        {
            var tableEntity = ConvertToTableEntity(entity);
            await _httpClient.PostAsJsonAsync($"{_baseUrl}/{_tableName}", tableEntity);
        }

        public async Task UpdateAsync(T entity)
        {
            var tableEntity = ConvertToTableEntity(entity);
            await _httpClient.PostAsJsonAsync($"{_baseUrl}/{_tableName}", tableEntity);
        }

        private TableEntity ConvertToTableEntity(T entity)
        {
            var tableEntity = new TableEntity();

            var pkProp = typeof(T).GetProperty("PartitionKey");
            var rkProp = typeof(T).GetProperty("RowKey");

            if (pkProp == null || rkProp == null)
                throw new Exception("Model must have PartitionKey and RowKey properties");

            tableEntity.PartitionKey = pkProp.GetValue(entity)?.ToString() ?? Guid.NewGuid().ToString();
            tableEntity.RowKey = rkProp.GetValue(entity)?.ToString() ?? Guid.NewGuid().ToString();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.Name == "PartitionKey" || prop.Name == "RowKey") continue;
                tableEntity[prop.Name] = prop.GetValue(entity);
            }

            return tableEntity;
        }

        public async Task DeleteAsync(string rowKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{_tableName}/{rowKey}");
            await _httpClient.SendAsync(request);
        }
    }
}
