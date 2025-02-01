using System.Diagnostics.CodeAnalysis;

namespace Durable.Functions.Models
{
    [ExcludeFromCodeCoverage]
    public class DurableResponse
    {
        public string RuntimeStatus { get; set; }

        public string Output { get; set; }
    }
}
