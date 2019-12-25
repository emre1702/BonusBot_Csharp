using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grpc.Core;

namespace TDSConnectorServerAssembly
{
    public class MessageToChannelService : MessageToChannel.MessageToChannelBase
    {

        public override async Task<MessageToChannelRequestReply> Send(MessageToChannelRequest request, ServerCallContext context)
        {
            try
            {
                var client = Program.DiscordClient;

                var guild = client.GetGuild(request.GuildId);
                if (guild is null)
                    return new MessageToChannelRequestReply { ErrorMessage = $"The guild with Id {request.GuildId} does not exist." };

                if (!(guild.GetChannel(request.ChannelId) is SocketTextChannel channel))
                    return new MessageToChannelRequestReply { ErrorMessage = $"The channel with Id {request.ChannelId} does not exist." };

                await channel.SendMessageAsync(request.Text);

                return new MessageToChannelRequestReply { ErrorMessage = null };
            }
            catch (Exception ex)
            {
                return new MessageToChannelRequestReply
                {
                    ErrorMessage = ex.GetBaseException().Message
                };
            }
        }
    }
}
