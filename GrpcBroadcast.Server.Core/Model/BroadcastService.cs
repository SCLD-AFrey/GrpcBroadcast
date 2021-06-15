using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GrpcBroadcast.Common;
using GrpcBroadcast.Server.Infrastructure;
using GrpcBroadcast.Server.Model;

namespace GrpcBroadcast.Server.Model
{
    [Export]
    public class BroadcastService
    {
        [Import] private Logger m_logger;

        [Import] private IBroadcastLogRepository m_repository;

        private event Action<BroadcastLog> Added;

        public void Add(BroadcastLog broadcastLog)
        {
            m_logger.Info($"{broadcastLog}");

            m_repository.Add(broadcastLog);
            Added?.Invoke(broadcastLog);
        }

        public IObservable<BroadcastLog> GetChatLogsAsObservable()
        {
            var oldLogs = m_repository.GetAll().ToObservable();
            var newLogs = Observable.FromEvent<BroadcastLog>(x => Added += x, x => Added -= x);

            return oldLogs.Concat(newLogs);
        }
    }
}