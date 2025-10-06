using Azure;
using Azure.Data.Tables;

namespace C2B_Functions.Models
{
    public class Category : ITableEntity
    {
        public string PartitionKey { get; set; } = "Categories";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string? CatName { get; set; }
    }
}

