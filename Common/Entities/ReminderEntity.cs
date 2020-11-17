using System;

namespace BonusBot.Common.Entities
{
    public sealed class ReminderEntity : BaseEntity
    {
        public ulong UserId { get; set; }
        public string Content { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
    }
}
