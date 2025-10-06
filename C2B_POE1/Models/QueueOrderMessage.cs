using Azure;
using Azure.Data.Tables;

namespace C2B_POE1.Models
{
    public class QueueOrder : ITableEntity
    {
        public string PartitionKey { get; set; } = "Orders";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public bool Fufilled { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class QueueOrderLine : ITableEntity
    {
        public string PartitionKey { get; set; } = "Orders";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string ProductRowKey { get; set; } = null!;
        public int Quantity { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class QueueOrderMessage
    {
        public QueueOrder Order { get; set; } = new QueueOrder();
        public List<QueueOrderLine> OrderLines { get; set; } = new List<QueueOrderLine>();
    }
}