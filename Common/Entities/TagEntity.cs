using System;

namespace BonusBot.Common.Entities
{
    public sealed class TagEntity : BaseEntity
    {
        public int Uses { get; set; }
        public ulong GuildId { get; set; }
        public string Content { get; set; }
        public ulong OwnerId { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}