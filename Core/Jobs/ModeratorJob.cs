using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using BonusBot.Common.Entities;
using BonusBot.Common.Helpers;
using BonusBot.Common.Handlers;

namespace BonusBot.Core.Jobs
{
    public sealed class ModeratorJob : BaseJob
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseHandler _database;
        private readonly RolesHandler _rolesHandler;

        public ModeratorJob(DiscordSocketClient client, DatabaseHandler databaseHandler, RolesHandler rolesHandler)
        {
            _client = client;
            _database = databaseHandler;
            _rolesHandler = rolesHandler;
        }

        protected override async Task RunAsync()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                var cases = _database.GetCollection<CaseEntity>().FindAll()
                    .Where(x => x.ExpiresOn < DateTimeOffset.Now && x.IsTempCase());

                foreach (var userCase in cases)
                {
                    try
                    {
                        var guild = _client.GetGuild(userCase.GuildId);
                        var guildDb = _database.Get<GuildEntity>(guild.Id);
                        var user = guild.GetUser(userCase.UserId);

                        switch (userCase.CaseType)
                        {
                            case CaseType.TempBan:
                                await guild.RemoveBanAsync(userCase.UserId);
                                break;

                            case CaseType.TempMute:
                                var muteRole = guild.GetRole(guildDb.MuteRoleId);
                                if (user != null)
                                    _rolesHandler.ChangeRolesToUnmute(user);
                                break;

                            case CaseType.TempBlock:
                                var channel = guild.GetTextChannel(userCase.ChannelId);
                                if (user != null)
                                    await channel.RemovePermissionOverwriteAsync(user);
                                break;
                        }

                        //var modChannel = guild.GetTextChannel(guildDb.LogChannelId);
                        //await modChannel?.SendMessageAsync("");
                    }
                    catch
                    {
                        //ignored
                    }

                    _database.Delete(userCase);
                }
                await Task.Delay(TaskDelay);
            }
        }
    }
}
