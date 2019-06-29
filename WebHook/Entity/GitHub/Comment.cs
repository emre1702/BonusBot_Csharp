using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Comment
    {
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        /*[JsonProperty("user")]
        public User User { get; set; }*/

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
