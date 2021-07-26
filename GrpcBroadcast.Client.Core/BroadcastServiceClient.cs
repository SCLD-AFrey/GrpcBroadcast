using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcBroadcast.Common;

namespace GrpcBroadcast.Client.Core
{
    public class BroadcastServiceClient
    {
        private readonly Broadcast.BroadcastClient m_client;

        public BroadcastServiceClient()
        {
            m_client = new Broadcast.BroadcastClient(
                new Channel("localhost", 50052, ChannelCredentials.Insecure));
        }

        public async Task Write(BroadcastLog broadcastLog)
        {
            await m_client.WriteAsync(broadcastLog);
        }

        public IAsyncEnumerable<BroadcastLog> BroadcastLogs()
        {
            var call = m_client.Subscribe(new Empty());

            // I do not want to expose gRPC such as IAsyncStreamReader or AsyncServerStreamingCall.
            // I also do not want to bother user of this class with asking to dispose the call object.

            return call.ResponseStream
                .ToAsyncEnumerable()
                .Finally(() => call.Dispose());
        }
        
        public async void WriteCommandExecute(string content, string m_originId)
        {
            await Write(new BroadcastLog
            {
                OriginId = m_originId,
                Content = content,
                At = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            });
        }
    }
}