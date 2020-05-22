using System;
using Common.Interfaces;
using Grpc.Net.Client;
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
            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5001");
            Command = new TDSClientCommand(grpcChannel);
            SupportRequest = new TDSClientSupportRequest(grpcChannel);
        }


    }
}
