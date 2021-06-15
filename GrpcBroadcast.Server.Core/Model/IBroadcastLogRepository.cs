using System.Collections.Generic;
using GrpcBroadcast.Common;

namespace GrpcBroadcast.Server.Model
{
    public interface IBroadcastLogRepository
    {
        void Add(BroadcastLog chatLog);
        IEnumerable<BroadcastLog> GetAll();
    }
}