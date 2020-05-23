using System;
using BonusBot.Common.Helpers;
using Discord;
using Discord.WebSocket;

namespace BonusBot.Common.Entities
{
    public sealed class CaseEntity : BaseEntity
    {
        public string Reason { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ModeratorId { get; set; }
        public CaseType CaseType { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public EmbedBuilder ToEmbedBuilder(DiscordSocketClient client)
        {
            var guild = client?.GetGuild(GuildId);
            var moderator = guild?.GetUser(ModeratorId);
            var builder = new EmbedBuilder();
            bool isTemporary = this.IsTempCase();
            builder.AddField("Type:", CaseType.ToString(), true);
            if (moderator != null)
                builder.AddField("Admin:", moderator.Username, true);
            builder.AddField("Created:", CreatedOn.ToLocalTime().ValueToString(), true);
            builder.AddField("Expires:", isTemporary ? ExpiresOn.ToLocalTime().ValueToString() : "-", true);
            builder.AddField("Reason:", Reason);
            return builder;
        }

        public Embed ToEmbed(DiscordSocketClient client)
        {
            return ToEmbedBuilder(client).Build();
        }
    }

    public enum CaseType
    {
        Ban,
        Kick,
        Mute,
        Block,
        TempBan,
        TempMute,
        TempBlock,
        RoleBan,
        TempRoleBan
    }
}
