using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.Handlers;
using Common.Enums;
using Common.Handlers;
using Discord;
using Discord.WebSocket;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace TDSConnectorServerAssembly
{
    public class SupportRequestService : SupportRequest.SupportRequestBase
    {
        public override async Task<SupportRequestCreateReply> Create(SupportRequestCreateRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new SupportRequestCreateReply
                    {
                        ErrorMessage = $"The guild with Id {request.GuildId} does not exist.",
                        ErrorStackTrace = Environment.StackTrace,
                        CreatedChannelId = 0
                    };

                var user = guild.GetUser(request.UserId);
                if (user is null)
                    return new SupportRequestCreateReply
                    {
                        ErrorMessage = $"The user with Id {request.UserId} does not exist.",
                        ErrorStackTrace = Environment.StackTrace,
                        CreatedChannelId = 0
                    };

                await Program.ServiceProvider.GetRequiredService<SupportRequestHandler>()
                    .CreateRequest(guild, user, request.AuthorName, request.Title, request.Text, (SupportType)request.SupportType, request.AtLeastAdminLevel, false);

                return new SupportRequestCreateReply
                {
                    ErrorMessage = string.Empty,
                    ErrorStackTrace = string.Empty,
                    CreatedChannelId = 0
                };
            }
            catch (Exception ex)
            {
                return new SupportRequestCreateReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace,
                    CreatedChannelId = 0
                };
            }
        }

        public override async Task<SupportRequestReply> Answer(SupportRequestAnswerRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var guildEntity = Program.ServiceProvider.GetRequiredService<DatabaseHandler>()
                    .Get<GuildEntity>(guild.Id);
                if (guildEntity is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var categoryId = guildEntity.SupportRequestCategoryId;
                if (categoryId == 0)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
                var supportRequestCategory = guild.GetCategoryChannel(categoryId);
                if (supportRequestCategory is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var channel = supportRequestCategory.Channels
                    .OfType<SocketTextChannel>()
                    .FirstOrDefault(c => c.Name.EndsWith("support_" + request.SupportRequestId));
                if (channel is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var supportRequestHandler = Program.ServiceProvider.GetRequiredService<SupportRequestHandler>();
                if (channel.Name.StartsWith("closed-"))
                    await supportRequestHandler.ToggleClosedRequest(channel, null, request.AuthorName, false, false);

                request.Text = $"Answer from {request.AuthorName}:\n" + request.Text;

                await supportRequestHandler.AnswerRequest(channel, null, request.AuthorName, request.Text, false);

                return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
            }
            catch (Exception ex)
            {
                return new SupportRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace
                };
            }
        }

        public override async Task<SupportRequestReply> ToggleClosed(SupportRequestToggleClosedRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var guildEntity = Program.ServiceProvider.GetRequiredService<DatabaseHandler>()
                    .Get<GuildEntity>(guild.Id);
                if (guildEntity is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var categoryId = guildEntity.SupportRequestCategoryId;
                if (categoryId == 0)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
                var supportRequestCategory = guild.GetCategoryChannel(categoryId);
                if (supportRequestCategory is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                var channel = supportRequestCategory.Channels
                    .OfType<SocketTextChannel>()
                    .FirstOrDefault(c => c.Name.EndsWith("support_" + request.SupportRequestId));
                if (channel is null)
                    return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };

                await Program.ServiceProvider.GetRequiredService<SupportRequestHandler>()
                    .ToggleClosedRequest(channel, null, request.RequesterName, request.Closed, false);

                return new SupportRequestReply { ErrorMessage = string.Empty, ErrorStackTrace = string.Empty };
            }
            catch (Exception ex)
            {
                return new SupportRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message,
                    ErrorStackTrace = ex.StackTrace ?? Environment.StackTrace
                };
            }
        }

        private string GetUniversalDateTimeString(DateTimeOffset dateTime)
        {
            var enUsCulture = CultureInfo.CreateSpecificCulture("en-US");
            return dateTime.ToString("f", enUsCulture) + " +00:00";
        }
    }
}
