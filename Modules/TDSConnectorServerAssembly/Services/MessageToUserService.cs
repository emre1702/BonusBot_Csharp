using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace TDSConnectorServerAssembly
{
    public class MessageToUserService : MessageToUser.MessageToUserBase
    {

        public override async Task<MessageToUserRequestReply> Send(MessageToUserRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new MessageToUserRequestReply { ErrorMessage = $"The guild with Id {request.GuildId} does not exist.", ErrorStackTrace = Environment.StackTrace };

                SocketGuildUser? target = guild.GetUser(request.UserId);

                if (target is null)
                    return new MessageToUserRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var privateChat = await target.GetOrCreateDMChannelAsync();
                await privateChat.SendMessageAsync(request.Text);

                return new MessageToUserRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
            }
            catch (Exception ex)
            {
                return new MessageToUserRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace
                };
            }
        }

        public override async Task<MessageToUserRequestReply> SendEmbed(EmbedToUserRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new MessageToUserRequestReply { ErrorMessage = $"The guild with Id {request.GuildId} does not exist.", ErrorStackTrace = Environment.StackTrace };

                SocketGuildUser? target = guild.GetUser(request.UserId);
                if (target is null)
                    return new MessageToUserRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var embedBuilder = new EmbedBuilder();

                if (!string.IsNullOrEmpty(request.Title))
                    embedBuilder.WithTitle(request.Title);

                if (!string.IsNullOrEmpty(request.Author))
                    embedBuilder.WithAuthor(request.Author);

                embedBuilder.WithTimestamp(DateTimeOffset.UtcNow);

                foreach (var field in request.Fields)
                    embedBuilder.AddField(field.Name, field.Value);

                if (request.ColorR != -1 || request.ColorG != -1 || request.ColorB != -1)
                    embedBuilder.WithColor(request.ColorR != -1 ? request.ColorR : 255, request.ColorG != -1 ? request.ColorG : 255, request.ColorB != -1 ? request.ColorB : 255);

                var privateChat = await target.GetOrCreateDMChannelAsync();
                await privateChat.SendMessageAsync(embed: embedBuilder.Build());

                return new MessageToUserRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
            }
            catch (Exception ex)
            {
                return new MessageToUserRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace
                };
            }
        }
    }
}
