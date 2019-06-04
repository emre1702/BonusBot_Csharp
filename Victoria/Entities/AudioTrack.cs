using System;
using Discord.WebSocket;
using Victoria.Entities;

namespace Victoria.Entities
{
    public class AudioTrack
    {
        public LavaTrack Audio;
        public SocketGuildUser User;
        public DateTime TimeAdded = DateTime.Now;
        public LavaPlayer Player;

        public override string ToString()
        {
            return Audio.Title + " (" + Audio.Author + ")";
        }
    }
}
