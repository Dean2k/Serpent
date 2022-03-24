using Newtonsoft.Json;

namespace ReModCE_ARES.Core
{
    internal class ApiError
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
    }
}
