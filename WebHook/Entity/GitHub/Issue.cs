using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Issue
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        //[JsonProperty("user")]
        //public User User { get; set; }
    }
}
