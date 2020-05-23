using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using BonusBot.Common.Entities;
using BonusBot.Common.Handlers;

namespace BonusBot.Core.Jobs
{
    public sealed class ReminderJob : BaseJob
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseHandler _database;

        public ReminderJob(DiscordSocketClient client, DatabaseHandler databaseHandler)
        {
            _client = client;
            _database = databaseHandler;
        }

        protected override async Task RunAsync()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                _database.GetCollection<ReminderEntity>(collection => {
                    var reminders = collection.FindAll()
                        .Where(x => x.ExpiresOn <= DateTimeOffset.Now).ToList();

                    foreach (var reminder in reminders)
                    {
                        var guild = _client.GetGuild(reminder.GuildId);
                        var user = guild.GetUser(reminder.UserId);
                        var channel = guild.GetTextChannel(reminder.ChannelId);

                        switch (channel)
                        {
                            case null when user is null:
                                break;

                            case null when !(user is null):
                                user.SendMessageAsync(reminder.Content).Wait();
                                break;

                            default:
                                channel.SendMessageAsync(reminder.Content).Wait();
                                break;
                        }

                        _database.Delete(reminder);
                    }
                }).Wait();

                await Task.Delay(TaskDelay);
            }
        }
    }
}
