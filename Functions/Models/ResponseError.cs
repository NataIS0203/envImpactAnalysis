using Newtonsoft.Json;

namespace Durable.Models
{
    public class ResponseError
    {
        public ResponseError(string property = null, string message = null, int? errorCode = null)
        {
            Property = property;
            Message = message;
            ErrorCode = errorCode;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ErrorCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Property { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}
