using System;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using Discord.Rest;
using Discord.WebSocket;

namespace Common.Helpers
{
    public static class ChannelHelper
    {
        public static Task<RestTextChannel> CreateSupportChannel(SocketGuild guild, string channelName, ulong categoryId, bool createdInDiscord)
            => guild.CreateTextChannelAsync(channelName, properties =>
            {
                properties.CategoryId = categoryId;
                properties.Topic = $"Support request created in {(createdInDiscord ? "Discord" : "RAGE")}. Use only !answer [text] to answer!";
            });

        public static bool IsOnlyCommandsChannel(GuildEntity guild, ISocketMessageChannel channel)
            => channel.Id == guild.SupportRequestChannelInfoId 
            || ((channel is SocketTextChannel textChannel) && textChannel.CategoryId == guild.SupportRequestCategoryId);

    }
}
