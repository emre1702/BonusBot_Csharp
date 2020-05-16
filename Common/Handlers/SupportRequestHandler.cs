using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.Handlers;
using BonusBot.Helpers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Common.Handlers
{
    #nullable enable
    public class SupportRequestHandler
    {
        private readonly ITDSClient _tdsClient;
        private readonly DatabaseHandler _databaseHandler;

        public SupportRequestHandler(ITDSClient tdsClient, DatabaseHandler databaseHandler)
        {
            _tdsClient = tdsClient;
            _databaseHandler = databaseHandler;
        }

        public async Task CreateRequest(SocketGuild guild, SocketGuildUser user, string authorName, string title, string text, SupportType supportType, int atLeastAdminLevel, bool fromDiscord)
        {
            GuildEntity guildEntity = _databaseHandler.Get<GuildEntity>(guild.Id);
            if (guildEntity is null)
                return;

            var categoryId = guildEntity.SupportRequestCategoryId;
            if (categoryId == 0)
                return;
            var supportRequestCategory = guild.GetCategoryChannel(categoryId);
            if (supportRequestCategory is null)
                return;

            var embed = EmbedHelper.GetSupportRequestEmbed(user, title, text, supportType);

            var textChannel = await ChannelHelper.CreateSupportChannel(guild, "support_discord", categoryId, fromDiscord);
            await textChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(
                viewChannel: PermValue.Allow, 
                sendMessages: PermValue.Allow, 
                readMessageHistory: PermValue.Allow, 
                connect: PermValue.Allow, 
                speak: PermValue.Allow
            ));

            if (atLeastAdminLevel > 1 && guildEntity.SupporterRoleId != 0)
            {
                
                var supporterRole = guild.GetRole(guildEntity.SupporterRoleId);
                if (supporterRole is { })
                {
                    await textChannel.AddPermissionOverwriteAsync(supporterRole, OverwritePermissions.DenyAll(textChannel));
                }
            }
            if (atLeastAdminLevel > 2 && guildEntity.AdministratorRoleId != 0)
            {

                var administratorRole = guild.GetRole(guildEntity.AdministratorRoleId);
                if (administratorRole is { })
                {
                    await textChannel.AddPermissionOverwriteAsync(administratorRole, OverwritePermissions.DenyAll(textChannel));
                }
            }

            var message = await textChannel.SendMessageAsync(user.Mention, embed: embed.Build());
            await message.PinAsync();

            await user.SendMessageAsync($"Your support request has been sent, a channel has been created:\n{message.GetJumpUrl()}");

            if (fromDiscord)
            {
                var reply = await _tdsClient.UsedCommand(user.Id, "CreateSupportRequest", new List<string> { title, text, supportType.ToString("D"), atLeastAdminLevel.ToString() });
                if (int.TryParse(reply, out int requestId))
                {
                    await textChannel.ModifyAsync(c => c.Name = "support_" + requestId);
                    await user.SendMessageAsync($"The support request was successfully added to TDS-V.");
                }
                else
                    await user.SendMessageAsync($"The support request could not be added to TDS-V." +
                        $"\nYou either don't have an account yet or have not set your Discord-ID in your settings (correctly)." +
                        $"\nIf you've set the wrong id: You need to set your ID to '{user.Id}'");
            }
        }

        public async Task<bool> AnswerRequest(SocketGuild guild, SocketTextChannel channel, SocketGuildUser? user, string authorName, string text, bool fromDiscord)
        {
            GuildEntity guildEntity = _databaseHandler.Get<GuildEntity>(guild.Id);
            if (guildEntity is null)
                return false;
            
            if (!fromDiscord)
            {
                await channel.SendMessageAsync(authorName + ": " + text);
                return true;
            }
            else if (user is { })
            {
                var supportRequestIdStr = channel.Name.Substring("support_".Length);
                if (!int.TryParse(supportRequestIdStr, out int supportRequestId))
                    return false;

                var reply = await _tdsClient.UsedCommand(user.Id, "AnswerSupportRequest", new List<string> { supportRequestId.ToString(), text });
                if (reply == "-")
                {
                    await user.SendMessageAsync("The support request does not exist anymore.");
                    await channel.DeleteAsync();
                }
                else if (!string.IsNullOrEmpty(reply))
                {
                    await user.SendMessageAsync($"The answer could not be added to TDS-V." +
                        $"\nYou either don't have an account yet or have not set your Discord-ID in your settings (correctly)." +
                        $"\nIf you've set the wrong id: You need to set your ID to '{user.Id}'" +
                        $"\nError message: {reply}" +
                        $"\nYour answer:");
                    await user.SendMessageAsync(text);
                    return false;
                }
                return true;
                    
            }     
            return false;
        }
    }
}
