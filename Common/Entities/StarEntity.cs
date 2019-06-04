using System.Collections.Generic;

namespace BonusBot.Common.Entities
{
    public sealed class StarEntity : BaseEntity
    {
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong MessageId { get; set; }
        public ulong StarredId { get; set; }
        public List<ulong> Stargazers { get; set; }
    }
}