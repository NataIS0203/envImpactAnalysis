
namespace Durable.Functions.Models
{
    public class GetBaseRequest
    {
        public required string Name { get; set; }

        public string? Region { get; set; }

        public string? Percentage { get; set; }

        public string? Additional { get; set; }

        public string? ImageType { get; set; }

        public string? ReportName { get; set; }
    }
}
