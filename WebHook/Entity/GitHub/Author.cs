using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("email")]
        //[JsonProperty("username")]
    }
}
