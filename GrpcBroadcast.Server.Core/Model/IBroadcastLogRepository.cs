using System.Collections.Generic;
using GrpcBroadcast.Common;

namespace GrpcBroadcast.Server.Core.Model
{
    public interface IBroadcastLogRepository
    {
        void Add(BroadcastLog chatLog);
        IEnumerable<BroadcastLog> GetAll();
    }
}