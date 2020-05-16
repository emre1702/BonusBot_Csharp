using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.Handlers;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace BonusBot.Common.Handlers
{
    public class RoleReactionHandler : IHandler
    {
        private readonly RolesHandler _rolesHandler;
        private readonly DatabaseHandler _databaseHandler;
        private readonly DiscordSocketClient _botClient;

        private readonly Dictionary<string, PropertyInfo> _roleByEmojiName = new Dictionary<string, PropertyInfo>
        {
            ["👋"] = typeof(GuildEntity).GetProperty("MemberRoleId"),
            ["🇩🇪"] = typeof(GuildEntity).GetProperty("GermanRoleId"),
            ["🇹🇷"] = typeof(GuildEntity).GetProperty("TurkishRoleId"),
            ["📝"] = typeof(GuildEntity).GetProperty("DevFeedRoleId"),
            ["\U0001f9e0"] = typeof(GuildEntity).GetProperty("TesterRoleId"),
        };
        private readonly Dictionary<string, Emoji> _emojiByEmojiName = new Dictionary<string, Emoji>
        {
            ["👋"] = new Emoji("\ud83d\udc4b"),
            ["🇩🇪"] = new Emoji("\ud83c\udde9\ud83c\uddea"),
            ["🇹🇷"] = new Emoji("\ud83c\uddf9\ud83c\uddf7"),
            ["📝"] = new Emoji("\ud83d\udcdd"),
            ["\U0001f9e0"] = new Emoji("\uD83E\uDDE0")
        };
        private readonly Dictionary<ulong, HashSet<string>> _rolesUsedByGuild = new Dictionary<ulong, HashSet<string>>();

        public RoleReactionHandler(RolesHandler rolesHandler, DatabaseHandler databaseHandler, DiscordSocketClient botClient)
        {
            _rolesHandler = rolesHandler;
            _databaseHandler = databaseHandler;
            _botClient = botClient;
        }

        public async void InitForGuild(SocketGuild guild, GuildEntity guildEntity)
        {
            if (_rolesUsedByGuild.ContainsKey(guild.Id))
                _rolesUsedByGuild.Remove(guild.Id);
            var list = new HashSet<string>();
            _rolesUsedByGuild.Add(guild.Id, list);

            foreach (var entry in _roleByEmojiName)
            {
                ulong roleId = (ulong) entry.Value.GetValue(guildEntity);
                if (roleId == default)
                    continue;
                list.Add(entry.Key);
            }

            if (!guildEntity.UseRolesCommandSystem)
            {
                var rolesChannelId = guildEntity.RolesRequestChannelId;
                if (rolesChannelId == default)
                    return;
                var rolesChannel = guild.GetTextChannel(rolesChannelId);
                if (rolesChannel == null)
                    return;
                var pinnedMessages = await rolesChannel.GetPinnedMessagesAsync();
                if (pinnedMessages.Count == 0 || !pinnedMessages.Any(m => m.Author.Id == _botClient.CurrentUser.Id))
                    await CreateRoleReactionMessage(rolesChannel, guildEntity);
                else
                {
                    await EditRoleReactionMessage(rolesChannel.Guild.Id, pinnedMessages.Last() as RestUserMessage);
                }
            }

            if (guildEntity.SupportRequestChannelInfoId != 0 && !string.IsNullOrWhiteSpace(guildEntity.SupportRequestInfo))
            {
                await AddSupportInfoChannel(guild, guildEntity.SupportRequestChannelInfoId, guildEntity.SupportRequestInfo);
            }
        }

        public async Task HandleRoleReactionAdded(GuildEntity guild, SocketReaction reaction)
        {
            ulong guildId = Convert.ToUInt64(guild.Id);
            if (!_rolesUsedByGuild.ContainsKey(guildId))
                return;
            if (!_rolesUsedByGuild[guildId].Contains(reaction.Emote.Name))
                return;
            ulong roleId = (ulong)_roleByEmojiName[reaction.Emote.Name].GetValue(guild);
            if (roleId == default)
                return;
            var user = reaction.User.GetValueOrDefault();
            if (user == null || !(user is SocketGuildUser socketUser))
                return;

            string rolebanId = $"{user.Id}-{roleId}-RoleBan";
            var roleBan = _databaseHandler.Get<CaseEntity>(rolebanId);
            if (roleBan is { }) 
                return;
            await GiveRole(socketUser, roleId);
        }

        public async Task HandleRoleReactionRemoved(GuildEntity guild, SocketReaction reaction)
        {
            ulong guildId = Convert.ToUInt64(guild.Id);
            if (!_rolesUsedByGuild.ContainsKey(guildId))
                return;
            if (!_rolesUsedByGuild[guildId].Contains(reaction.Emote.Name))
                return;
            ulong roleId = (ulong)_roleByEmojiName[reaction.Emote.Name].GetValue(guild);
            if (roleId == default)
                return;
            var user = reaction.User.GetValueOrDefault();
            if (user == null || !(user is SocketGuildUser socketUser))
                return;
            await RemoveRole(socketUser, roleId);
        }

        private async Task GiveRole(SocketGuildUser user, ulong roleId)
        {
            var role = _rolesHandler.GetRole(user, roleId);
            if (role == null)
                return;
            await user.AddRoleAsync(role);
        }

        private async Task RemoveRole(SocketGuildUser user, ulong roleId)
        {
            var role = _rolesHandler.GetRole(user, roleId);
            if (role == null)
                return;
            await user.RemoveRoleAsync(role);
        }

        private async Task CreateRoleReactionMessage(SocketTextChannel rolesChannel, GuildEntity guild)
        {
            var infoText = !string.IsNullOrEmpty(guild.RolesRequestInfoText) ? guild.RolesRequestInfoText.Replace() : "Choose a reaction to get or remove the role.";
            var message = await rolesChannel.SendMessageAsync(infoText);
            var emojis = _rolesUsedByGuild[rolesChannel.Guild.Id]
                .Select(emojiStr => _emojiByEmojiName[emojiStr])
                .ToArray();
            await message.AddReactionsAsync(emojis);
            await message.PinAsync();
        }

        private async Task EditRoleReactionMessage(ulong guildId, RestUserMessage msg)
        {
            var emojisToAdd = _rolesUsedByGuild[guildId]
               .Select(emojiStr => _emojiByEmojiName[emojiStr])
               .ToArray();
            await msg.AddReactionsAsync(emojisToAdd);
        }

        private async Task AddSupportInfoChannel(SocketGuild guild, ulong channelId, string text)
        {
            var channel = guild.GetTextChannel(channelId);
            if (channel is null)
                return;

            var messages = await channel.GetPinnedMessagesAsync();
            
            if (messages.Count == 0)
            {
                var newMessage = await channel.SendMessageAsync(text.Replace());
                await newMessage.PinAsync();
            }
            
        }
    }
}
