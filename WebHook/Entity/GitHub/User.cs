using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class User
    {
        [JsonProperty("login")]
        public string Username { get; set; }

        [JsonProperty("url")]
        public string UserUrl { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }



        //[JsonProperty("id")]
        //[JsonProperty("node_id")]
        //[JsonProperty("gravatar_id")]
        //[JsonProperty("html_url")]
        //[JsonProperty("followers_url")]
        //[JsonProperty("following_url")]
        //[JsonProperty("gists_url")]
        //[JsonProperty("starred_url")]
        //[JsonProperty("subscriptions_url")]
        //[JsonProperty("organizations_url")]
        //[JsonProperty("repos_url")]
        //[JsonProperty("events_url")]
        //[JsonProperty("received_events_url")]
        //[JsonProperty("type")]
        //[JsonProperty("site_admin")]
    }
}
