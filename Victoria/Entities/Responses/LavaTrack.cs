﻿using Newtonsoft.Json;
using System;

namespace Victoria.Entities
{
    public sealed class LavaTrack
    {
        [JsonIgnore]
        internal string Hash { get; set; }

        [JsonProperty("identifier")]
        public string Id { get; internal set; }

        [JsonProperty("isSeekable")]
        public bool IsSeekable { get; internal set; }

        [JsonProperty("author")]
        public string Author { get; internal set; }

        [JsonProperty("isStream")]
        public bool IsStream { get; internal set; }

        [JsonIgnore]
        public TimeSpan Position
        {
            get => new TimeSpan(TrackPosition);
            internal set => TrackPosition = value.Ticks;
        }

        [JsonProperty("position")]
        internal long TrackPosition { get; set; }

        [JsonIgnore]
        public TimeSpan Length
            => TimeSpan.FromMilliseconds(TrackLength);

        [JsonProperty("length")]
        internal long TrackLength { get; set; }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("uri")]
        public Uri Uri { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public void ResetPosition()
        {
            Position = TimeSpan.Zero;
        }

        public override string ToString()
        {
            return Title + "(" + Author + ")";
        }
    }
}
