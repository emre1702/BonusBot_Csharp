using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums;
using Common.Interfaces;
using Grpc.Core;
using static TDSConnectorClient.SupportRequest;

namespace TDSConnectorClient.Requests
{
    public class TDSClientSupportRequest : ITDSClientSupportRequest
    {
        private readonly SupportRequestClient _client;

        internal TDSClientSupportRequest(ChannelBase grpcChannel)
        {
            _client = new SupportRequestClient(grpcChannel);
        }

        public async Task<string> Answer(ulong userId, int supportRequestId, string text)
        {
            var request = new AnswerRequest
            {
                UserId = userId,
                SupportRequestId = supportRequestId,
                Text = text
            };
            var reply = await _client.AnswerAsync(request);
            return reply.Message;
        }

        public async Task<string> Create(ulong userId, string title, string text, SupportType supportType, int atleastAdminLevel)
        {
            var request = new CreateRequest
            {
                UserId = userId,
                Title = title,
                Text = text,
                Type = (int)supportType,
                AtleastAdminLevel = atleastAdminLevel
            };
            var reply = await _client.CreateAsync(request);
            return reply.Message;
        }

        public async Task<string> ToggleClosed(ulong userId, int supportRequestId, bool closed)
        {
            var request = new ToggleClosedRequest
            {
                UserId = userId,
                SupportRequestId = supportRequestId,
                Closed = closed
            };
            var reply = await _client.ToggleClosedAsync(request);
            return reply.Message;
        }
    }
}
