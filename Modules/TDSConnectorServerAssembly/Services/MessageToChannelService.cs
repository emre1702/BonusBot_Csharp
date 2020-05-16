﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Helpers;
using Discord;
using Discord.WebSocket;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace TDSConnectorServerAssembly
{
    public class MessageToChannelService : MessageToChannel.MessageToChannelBase
    {

        public override async Task<MessageToChannelRequestReply> Send(MessageToChannelRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new MessageToChannelRequestReply { ErrorMessage = $"The guild with Id {request.GuildId} does not exist.", ErrorStackTrace = Environment.StackTrace };

                if (!(guild.GetChannel(request.ChannelId) is SocketTextChannel channel))
                    return new MessageToChannelRequestReply { ErrorMessage = $"The channel with Id {request.ChannelId} does not exist.", ErrorStackTrace = Environment.StackTrace };

                int maxSize = DiscordConfig.MaxMessageSize - 50;    // 50 just to be sure
                var texts = StringHelper.SplitByLength(request.Text, maxSize);

                foreach (var text in texts)
                {
                    await channel.SendMessageAsync(text);
                }

                return new MessageToChannelRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
            }
            catch (Exception ex)
            {
                return new MessageToChannelRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace
                };
            }
        }

        public override async Task<MessageToChannelRequestReply> SendEmbed(EmbedToChannelRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new MessageToChannelRequestReply { ErrorMessage = $"The guild with Id {request.GuildId} does not exist.", ErrorStackTrace = Environment.StackTrace };

                if (!(guild.GetChannel(request.ChannelId) is SocketTextChannel channel))
                    return new MessageToChannelRequestReply { ErrorMessage = $"The channel with Id {request.ChannelId} does not exist.", ErrorStackTrace = Environment.StackTrace };

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


                await channel.SendMessageAsync(embed: embedBuilder.Build());

                return new MessageToChannelRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
            }
            catch (Exception ex)
            {
                return new MessageToChannelRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace
                };
            }
        }
    }
}
