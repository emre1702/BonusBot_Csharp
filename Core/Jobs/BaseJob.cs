using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;

namespace BonusBot.Core.Jobs
{
    public abstract class BaseJob : IJob, IDisposable
    {
        private Task _runningTask;
        protected CancellationTokenSource CancellationTokenSource;
        protected TimeSpan TaskDelay = TimeSpan.FromSeconds(5);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task InitializeAsync()
        {
            if (!(_runningTask is null))
                return Task.CompletedTask;

            CancellationTokenSource = new CancellationTokenSource();
            _runningTask = RunAsync();

            ConsoleHelper.Log(LogSeverity.Info, "Job", $"Initialized {GetType().Name.Split('J')[0]} job with {TaskDelay.Seconds}s delay.");
            return Task.CompletedTask;
        }

        protected abstract Task RunAsync();

        public void ChangeDelay(TimeSpan delay)
        {
            TaskDelay = delay;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            CancellationTokenSource.Cancel(false);
            CancellationTokenSource.Dispose();
        }
    }
}
