using Azure;
using Azure.Data.Tables;

namespace C2B_POE1.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string ProductName { get; set; }
        public  string ProductDescription { get; set; }
        public double ProductPrice { get; set; }
        public string ProductImageURL { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;

    }
}
