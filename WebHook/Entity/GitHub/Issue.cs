using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Issue
    {
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        //[JsonProperty("user")]
        //public User User { get; set; }
    }
}
