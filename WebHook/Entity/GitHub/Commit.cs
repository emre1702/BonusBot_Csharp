using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Commit
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        //[JsonProperty("committer")]
        //public Author Commiter { get; set; }

        //[JsonProperty("tree_id")]
        //[JsonProperty("distinct")]
        //[JsonProperty("author")]
        //[JsonProperty("added")]
        //[JsonProperty("removed")]
        //[JsonProperty("modified")]
    }
}
