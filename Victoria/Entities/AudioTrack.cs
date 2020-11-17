using System;
using Discord.WebSocket;
using Victoria.Entities;

namespace Victoria.Entities
{
    public class AudioTrack
    {
        public LavaTrack Audio { get; set; }
        public SocketGuildUser User { get; set; }
        public DateTime TimeAdded { get; set; } = DateTime.Now;
        public LavaPlayer Player { get; set; }

        public override string ToString()
        {
            return Audio.Title + " (" + Audio.Author + ")";
        }
    }
}
