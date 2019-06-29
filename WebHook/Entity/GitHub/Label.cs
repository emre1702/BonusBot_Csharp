using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Label
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string HexColor { get; set; }
    }
}
