using Newtonsoft.Json;

namespace Durable.Models
{
    public class Response
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ResponseError> Errors { get; set; }
    }
}
