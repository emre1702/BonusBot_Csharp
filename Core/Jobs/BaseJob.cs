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

        public async Task InitializeAsync()
        {
            // CancellationTokenSource was cancelled, but the task didn't check it yet?
            // So wait for the task to finish, so it doesn't use the new CancellationTokenSource
            // and thinks it's still running
            if (!(_runningTask is null))
            {
                ConsoleHelper.Log(LogSeverity.Info, "Job", $"Waiting for {GetType().Name.Split('J')[0]} job to finish.");
                await _runningTask;
            }
                

            CancellationTokenSource = new CancellationTokenSource();
            _runningTask = RunAsync();

            ConsoleHelper.Log(LogSeverity.Info, "Job", $"Initialized {GetType().Name.Split('J')[0]} job.");
            return;
        }

        protected abstract Task RunAsync();

        public void ChangeDelay(TimeSpan delay)
        {
            TaskDelay = delay;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            CancellationTokenSource.Cancel(false);
            CancellationTokenSource.Dispose();
            _runningTask = null;
        }
    }
}
