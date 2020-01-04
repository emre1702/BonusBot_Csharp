using System;
using BonusBot.Common.ExtendedModules;
using Discord.Commands;
using Grpc.Net.Client;
using static TDSAssembly.BBCommand;

namespace TDSAssembly
{
    public partial class TDSModule : CommandBase
    {
        private static BBCommandClient _connectorClient; 

        protected override void BeforeExecute(CommandInfo command)
        {
            base.BeforeExecute(command);

            if (_connectorClient is { })
                return;

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5001");
            _connectorClient = new BBCommandClient(grpcChannel);
        }
    }
}
