using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces;
using Grpc.Core;
using static TDSConnectorClient.BBCommand;

namespace TDSConnectorClient.Requests
{
    public class TDSClientCommand : ITDSClientCommand
    {
        private BBCommandClient _client;

        internal TDSClientCommand(ChannelBase grpcChannel)
        {
            _client = new BBCommandClient(grpcChannel);
        }

        public async Task<string> UsedCommand(ulong userId, string command, List<string>? args = null)
        {
            var data = new UsedCommandRequest
            {
                UserId = userId,
                Command = command
            };
            if (args is { })
                data.Args.AddRange(args);
            return (await _client.UsedCommandAsync(data)).Message;
        }
    }
}
