using Common.Interfaces;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using TDSConnectorClient.Requests;

namespace TDSConnectorClient
{
    public class TDSClient : ITDSClient
    {
        public ITDSClientCommand Command { get; }
        public ITDSClientSupportRequest SupportRequest { get; }

        public TDSClient()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var grpcChannel = GrpcChannel.ForAddress("http://ragemp-server:5001", channelOptions: new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Insecure
            });
            Command = new TDSClientCommand(grpcChannel);
            SupportRequest = new TDSClientSupportRequest(grpcChannel);
        }
    }
}