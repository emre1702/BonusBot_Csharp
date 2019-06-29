using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Body
    {
        [JsonProperty("from")]
        public string From { get; set; }
    }
}
