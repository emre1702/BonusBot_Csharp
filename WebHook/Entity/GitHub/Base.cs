using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Base
    {
        [JsonProperty("ref")]
        public string Ref { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("commits")]
        public Commit[] Commits { get; set; }

        [JsonProperty("head_commit")]
        public Commit HeadCommit { get; set; }



        [JsonIgnore]
        public string Branch => Ref.Split('/')[^1];
    }
}
