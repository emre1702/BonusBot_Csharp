using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums;
using Common.Interfaces;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using static TDSConnectorClient.BBCommand;

namespace TDSConnectorClient
{
    public class TDSClient : ITDSClient
    {
        private BBCommandClient _connectorClient;

        public TDSClient()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5001");
            _connectorClient = new BBCommandClient(grpcChannel);
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
            return (await _connectorClient.UsedCommandAsync(data)).Message;
        }
    }
}
