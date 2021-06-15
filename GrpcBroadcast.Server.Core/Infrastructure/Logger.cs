using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace GrpcBroadcast.Server.Core.Infrastructure
{
    [Export]
    public class Logger
    {
        private readonly List<string> m_logs = new();
        private readonly Subject<string> m_observableLogs = new();

        public IObservable<string> GetLogsAsObservable()
        {
            return m_logs.ToObservable().Concat(m_observableLogs);
        }

        public void Info(object obj, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "")
        {
            var log = $"{DateTime.Now}: {Path.GetFileName(filePath)} - {name}: {obj}";

            m_logs.Add(log);
            m_observableLogs.OnNext(log);
        }
    }
}