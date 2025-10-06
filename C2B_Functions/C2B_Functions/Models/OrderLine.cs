using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace C2B_Functions.Models
{
    public class OrderLine : ITableEntity
    {
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore] public DateTimeOffset? Timestamp { get; set; }
        [JsonIgnore] public ETag ETag { get; set; }
        public string ProductRowKey { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
