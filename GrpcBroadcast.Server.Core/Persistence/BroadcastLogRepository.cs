using System.Collections.Generic;
using System.ComponentModel.Composition;
using GrpcBroadcast.Common;
using GrpcBroadcast.Server.Model;

namespace GrpcBroadcast.Server.Persistence
{
    [Export(typeof(IBroadcastLogRepository))]
    public class BroadcastLogRepository : IBroadcastLogRepository
    {
        private readonly List<BroadcastLog> m_storage = new(); // dummy on memory storage

        public void Add(BroadcastLog chatLog)
        {
            m_storage.Add(chatLog);
        }

        public IEnumerable<BroadcastLog> GetAll()
        {
            return m_storage.AsReadOnly();
        }
    }
}