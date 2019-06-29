using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WebHook.Entity.GitHub
{
    class Changes
    {
        [JsonProperty("body")]
        public Body Body { get; set; }
    }
}
