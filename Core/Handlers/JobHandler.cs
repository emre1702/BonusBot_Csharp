using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using BonusBot.Core.Jobs;

namespace BonusBot.Core.Handlers
{
    public sealed class JobHandler : IHandler
    {
        private readonly ConcurrentBag<IJob> _jobs;
        private readonly IServiceProvider _provider;

        public JobHandler(IServiceProvider provider)
        {
            _provider = provider;
            _jobs = new ConcurrentBag<IJob>();
        }

        public async Task InitializeAsync()
        {
            var reminder = _provider.GetRequiredService<ReminderJob>();
            var moderator = _provider.GetRequiredService<ModeratorJob>();
            var metrics = _provider.GetRequiredService<MetricsJob>();

            await reminder.InitializeAsync();
            await moderator.InitializeAsync();

            metrics.ChangeDelay(TimeSpan.FromSeconds(3));
            await metrics.InitializeAsync();

            _jobs.Add(reminder);
            _jobs.Add(moderator);
            _jobs.Add(metrics);
        }

        public void CancelJobs()
        {
            foreach (var job in _jobs)
                job.CastTo<BaseJob>().Dispose();
        }
    }
}
