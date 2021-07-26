using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcBroadcast.Common;
using GrpcBroadcast.Server.Core.Infrastructure;
using GrpcBroadcast.Server.Core.Model;

namespace GrpcBroadcast.Server.Core.Rpc
{
    public class NoticationServiceGrpcServer
    {
        [Export(typeof(IService))]
        public class BroadcastServiceGrpcServer : Broadcast.BroadcastBase, IService
        {
            private const int Port = 50052;
            private readonly Empty m_empty = new();
            private readonly Grpc.Core.Server m_server;
            private ServerPort m_serverport;

            [Import] private Logger m_logger;

            [Import] private BroadcastService m_broadcastService;

            public BroadcastServiceGrpcServer()
            {
                // Locate required files and set true to enable SSL
                //var secure = false;

                m_serverport = new ServerPort("localhost", Port, ServerCredentials.Insecure);
                
                m_server = new Grpc.Core.Server
                {
                    Services =
                    {
                        Broadcast.BindService(this)
                            .Intercept(new IpAddressAuthenticator())
                    },
                    Ports =
                    {
                        m_serverport
                    }
                };
            }

            public void Start()
            {
                m_server.Start();

                m_logger.Info("Started.");
            }

            public override async Task Subscribe(Empty request, IServerStreamWriter<BroadcastLog> responseStream,
                ServerCallContext context)
            {
                var peer = context.Peer; // keep peer information because it is not available after disconnection
                m_logger.Info($"{peer} subscribes.");

                context.CancellationToken.Register(() => m_logger.Info($"{peer} cancels subscription."));

                // Completing the method means disconnecting the stream by server side.
                // If subscribing IObservable, you have to block this method after the subscription.
                // I prefer converting IObservable to IAsyncEnumerable to consume the sequense here
                // because gRPC interface is in IAsyncEnumerable world.
                // Note that the chat service model itself is in IObservable world
                // because chat is naturally recognized as an event sequence.

                try
                {
                    await m_broadcastService.GetChatLogsAsObservable()
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async x => await responseStream.WriteAsync(x), context.CancellationToken)
                        .ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    m_logger.Info($"{peer} unsubscribed.");
                }
            }

            public override Task<Empty> Write(BroadcastLog request, ServerCallContext context)
            {
                m_logger.Info($"{context.Peer} {request}");

                m_broadcastService.Add(request);

                return Task.FromResult(m_empty);
            }
        }
    }
}