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

            var textChannel = await ChannelHelper.CreateSupportChannel(guild, supportType.ToString() + "_discord", categoryId, fromDiscord);
            await textChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(
                viewChannel: PermValue.Allow, 
                sendMessages: PermValue.Allow, 
                readMessageHistory: PermValue.Allow, 
                connect: PermValue.Allow, 
                speak: PermValue.Allow
            ));

            if (atLeastAdminLevel > 1 && guildEntity.SupporterRoleId != 0)
                await textChannel.DenyAccess(guild, guildEntity.SupporterRoleId);
            if (atLeastAdminLevel > 2 && guildEntity.AdministratorRoleId != 0)
                await textChannel.DenyAccess(guild, guildEntity.AdministratorRoleId);

            var message = await textChannel.SendMessageAsync(user.Mention, embed: embed.Build());
            await message.PinAsync();

            await user.SendMessageAsync($"Your support request has been sent, a channel has been created:{Environment.NewLine}{message.GetJumpUrl()}");

            if (fromDiscord)
            {
                var reply = await _tdsClient.SupportRequest.Create(user.Id, title, text, supportType, atLeastAdminLevel);
                if (int.TryParse(reply, out int requestId))
                {
                    await textChannel.ModifyAsync(c => c.Name = "support_" + requestId);
                    await user.SendMessageAsync($"The support request was successfully added to TDS-V.");
                }
                else
                    await user.SendMessageAsync($"The support request could not be added to TDS-V." +
                        $"{Environment.NewLine}You either don't have an account yet or have not set your Discord-ID in your settings (correctly)." +
                        $"{Environment.NewLine}If you've set the wrong id: You need to set your ID to '{user.Id}'");
            }
        }

        public async Task<bool> AnswerRequest(SocketTextChannel channel, SocketGuildUser? user, string authorName, string text, bool fromDiscord)
        {
            if (!fromDiscord)
            {
                await channel.SendMessageAsync(authorName + ": " + text);
                return true;
            }
            else if (user is { })
            {
                var supportRequestIdStr = channel.Name.Substring(channel.Name.IndexOf('_') + 1);
                if (!int.TryParse(supportRequestIdStr, out int supportRequestId))
                    return false;

                var reply = await _tdsClient.SupportRequest.Answer(user.Id, supportRequestId, text);
                if (reply == "-")
                {
                    await user.SendMessageAsync("The support request does not exist anymore.");
                    await channel.DeleteAsync();
                    return false;
                }
                else if (!string.IsNullOrEmpty(reply))
                {
                    await user.SendMessageAsync($"The answer could not be added to TDS-V." +
                        $"{Environment.NewLine}You either don't have an account yet or have not set your Discord-ID in your settings (correctly)." +
                        $"{Environment.NewLine}If you've set the wrong id: You need to set your ID to '{user.Id}'" +
                        $"{Environment.NewLine}Error message: {reply}" +
                        $"{Environment.NewLine}Your answer:{Environment.NewLine}{text}");
                    await user.SendMessageAsync(text);
                    return false;
                }
                return true;
                    
            }     
            return false;
        }

        public async Task ToggleClosedRequest(SocketTextChannel channel, SocketGuildUser? user, string requesterName, bool close, bool fromDiscord)
        {
            var newName = close
                ? "closed-" + channel.Name
                : channel.Name.Substring("closed-".Length);
            
            await channel.ModifyAsync(p => p.Name = newName);
            await channel.SendMessageAsync($"The support request has been {(close ? "closed" : "opened")} by {requesterName}." +
                $"{Environment.NewLine}To {(close ? "open" : "close")} it again you can use the command \"!support {(close ? "open" : "close")}\".");

            if (fromDiscord)
            {
                if (!int.TryParse(channel.Name.Substring(channel.Name.IndexOf("_") + 1), out int supportRequestId))
                    return;
                await _tdsClient.SupportRequest.ToggleClosed(user!.Id, supportRequestId, close);
            }
                
        }

        public async Task HandleMessage(GuildEntity guild, SocketUserMessage message)
        {
            if (guild is null)
            {
                await message.DeleteAsync();
                return;
            }

            if (!(message.Channel is SocketTextChannel channel))
            {
                await message.DeleteAsync();
                return;
            }

            if (!(message.Author is SocketGuildUser user))
            {
                await message.DeleteAsync();
                return;
            }

            var content = message.Content;
            if (channel.Name.StartsWith("closed-"))
            {
                await message.DeleteAsync();
                var task = user?.SendMessageAsync("The request you've answered to is currently closed." +
                    $"{Environment.NewLine}Use \"!support open\" to open it again before answering." +
                    $"{Environment.NewLine}Your answer:{Environment.NewLine}{content}");
                if (task is { })
                    await task;
                return;
            }

            if (message.Content.Length > guild.SupportRequestMaxTextLength)
            {
                await message.DeleteAsync();
                await message.Author.SendMessageAsync($"The text can be a maximum of {guild.SupportRequestMaxTextLength} characters long." +
                    $"{Environment.NewLine}Your answer:{Environment.NewLine}{content}");
                return;
            }

            if (!await AnswerRequest(channel, user, user.Nickname, content, true))
                await message.DeleteAsync();
        }
    }
}
