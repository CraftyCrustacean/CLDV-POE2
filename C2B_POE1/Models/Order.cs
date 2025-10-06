using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace C2B_POE1.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore] public DateTimeOffset? Timestamp { get; set; }
        [JsonIgnore] public ETag ETag { get; set; }
        public bool Fufilled { get; set; }
    }
}
