using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using InfluxDB.Collector;
using BonusBot.Common.Helpers;

namespace BonusBot.Core.Jobs
{
    public sealed class MetricsJob : BaseJob
    {
        private readonly ConcurrentDictionary<string, object> _builder;
        private readonly Process _process;
        private readonly DateTime _lastTimeStamp;

        public MetricsJob()
        {
            _lastTimeStamp = DateTime.Now;
            _process = Process.GetCurrentProcess();
            _builder = new ConcurrentDictionary<string, object>();
        }

        protected override async Task RunAsync()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                CollectProcessStats();
                await Task.Delay(TaskDelay);
            }
        }

        public void CollectPing(int old, int @new)
        {
            _builder.TryAdd("Old", old);
            _builder.TryAdd("New", @new);
            Metrics.Write("Gateway Latency", _builder);
            _builder.Clear();
        }

        public void CollectProcessStats()
        {
            _builder.TryAdd("Gen 0", GC.CollectionCount(0));
            _builder.TryAdd("Gen 1", GC.CollectionCount(1));
            _builder.TryAdd("Gen 2", GC.CollectionCount(2));
            Metrics.Write("Garbage Collection", _builder);
            _builder.Clear();

            var uptime = _lastTimeStamp - _process.StartTime;
            _builder.TryAdd("Uptime", uptime.TotalSeconds);
            Metrics.Write("Process-Uptime", _builder);
            _builder.Clear();

            _builder.TryAdd("Private", _process.PrivateMemorySize64.ConvertToMb());
            _builder.TryAdd("Virtual", _process.VirtualMemorySize64.ConvertToMb());
            _builder.TryAdd("Paged", _process.PagedMemorySize64.ConvertToMb());
            Metrics.Write("Memory", _builder);
            _builder.Clear();
        }
    }
}
