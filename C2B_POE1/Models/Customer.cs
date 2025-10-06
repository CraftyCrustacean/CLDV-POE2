using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;

namespace C2B_POE1.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Required]
        public string CustomerFirstName { get; set; } = string.Empty;

        [Required]
        public string CustomerLastName { get; set; } = string.Empty;

        [Required]
        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerCell { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CustomerDoB { get; set; }
    }
}
