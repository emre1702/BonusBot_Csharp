using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using BonusBot.Common.Entities;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using BonusBot.Core.Jobs;
using Victoria;
using Victoria.Entities;
using BonusBot.Common.Handlers;
using BonusBot.WebHook;
using Common.Handlers;
using WebHook.Entity;
using WebHook;
using System.Collections.Generic;
using LiteDB;
using Common.Helpers;
using System.Threading.Channels;

namespace BonusBot.Core.Handlers
{
    public sealed class EventsHandler : IHandler
    {
        private readonly CommandService _commandService;
        private readonly JobHandler _jobHandler;
        private readonly MetricsJob _metricsJob;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _socketClient;
        private readonly LavaSocketClient _lavaSocketClient;
        private readonly DatabaseHandler _databaseHandler;
        private readonly RolesHandler _rolesHandler;
        private readonly RoleReactionHandler _roleReactionHandler;
        private readonly SupportRequestHandler _supportRequestHandler;

        public EventsHandler(IServiceProvider provider,
            DiscordSocketClient socketClient, CommandService commandService, MetricsJob metricsJob, JobHandler jobHandler,
            LavaSocketClient lavaSocketClient, DatabaseHandler databaseHandler, RolesHandler rolesHandler, RoleReactionHandler roleReactionHandler,
            SupportRequestHandler supportRequestHandler)
        {
            _socketClient = socketClient;
            _commandService = commandService;
            _serviceProvider = provider;
            _metricsJob = metricsJob;
            _jobHandler = jobHandler;
            _databaseHandler = databaseHandler;
            _rolesHandler = rolesHandler;
            _roleReactionHandler = roleReactionHandler;
            _supportRequestHandler = supportRequestHandler;

            _lavaSocketClient = lavaSocketClient;
            _lavaSocketClient.Log += OnLog;
            //_lavaSocketClient.OnPlayerUpdated += OnPlayerUpdated;
            //_lavaSocketClient.OnServerStats += OnServerStats;
            //_lavaSocketClient.OnSocketClosed += OnSocketClosed;
            //_lavaSocketClient.OnTrackException += OnTrackException;
            _lavaSocketClient.OnTrackFinished += OnTrackFinished;
            //_lavaSocketClient.OnTrackStuck += OnTrackStuck;

            socketClient.Log += OnLog;
            socketClient.Ready += OnReady;
            socketClient.UserJoined += OnUserJoined;
            socketClient.UserLeft += OnUserLeft;
            socketClient.Disconnected += OnDisconnected;
            socketClient.LatencyUpdated += OnLatencyUpdated;
            socketClient.MessageReceived += OnMessage;
            socketClient.ReactionAdded += OnReactionAdded;
            socketClient.ReactionRemoved += OnReactionRemoved;

            commandService.CommandExecuted += OnCommandExecuted;

            ModuleEventsHandler.GitHubWebHookSettingChanged += CreateGitHubListenerForGuild;
        }

        private Task OnPlayerUpdated(LavaPlayer player, LavaTrack track, TimeSpan position)
        {
            ConsoleHelper.Log(LogSeverity.Info, "Victoria", $"Player Updated For {player.VoiceChannel.GuildId}: {position}");
            return Task.CompletedTask;
        }

        private Task OnServerStats(ServerStats stats)
        {
            ConsoleHelper.Log(LogSeverity.Info, "Victoria", $"Uptime: {stats.Uptime}");
            return Task.CompletedTask;
        }

        private Task OnSocketClosed(int code, string reason, bool remote)
        {
            ConsoleHelper.Log(LogSeverity.Info, "Victoria", $"LavaSocket closed: {code} | {reason} | {remote}");
            return Task.CompletedTask;
        }

        private Task OnTrackException(LavaPlayer player, LavaTrack track, string error)
        {
            ConsoleHelper.Log(LogSeverity.Info, "Victoria", $"Player {player.VoiceChannel.GuildId} {error} for {track.Title}");
            return Task.CompletedTask;
        }

        private async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext())
                return;

            if (!player.Queue.TryDequeue(out var item) || !(item is AudioTrack nextTrack))
            {
                await player.TextChannel?.SendMessageAsync($"There are no more items left in queue.");
                return;
            }

            await player.PlayAsync(nextTrack);
            await player.TextChannel.SendMessageAsync($"Finished playing: {track.ToString()}\nNow playing: {nextTrack.ToString()}");

            await player.TextChannel.SendMessageAsync(player.ObjectToString());
        }

        private Task OnTrackStuck(LavaPlayer player, LavaTrack track, long threshold)
        {
            ConsoleHelper.Log(LogSeverity.Info, "Victoria", $"{track.Title} stuck after {threshold}ms for {player.VoiceChannel.GuildId}.");
            return Task.CompletedTask;
        }

        private Task OnLog(LogMessage log)
        {
            ConsoleHelper.Log(log.Severity, log.Source, log.Message, log.Exception);
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            _databaseHandler.VerifyGuilds(_socketClient.Guilds.Select(x => x.Id));
            await _jobHandler.InitializeAsync();
            await _lavaSocketClient.StartAsync(_socketClient, new Configuration
            {
                LogSeverity = LogSeverity.Warning,
                ReconnectAttempts = 5,
                ReconnectInterval = TimeSpan.FromSeconds(3)
            });

            var collection = _databaseHandler.GetCollection<CaseEntity>();
            foreach (var guild in _socketClient.Guilds)
            {
                var guildEntity = _databaseHandler.Get<GuildEntity>(guild.Id);
                _roleReactionHandler.InitForGuild(guild, guildEntity);
                CreateGitHubListenerForGuild(guild, guildEntity);
                await CheckCaseEntities(guild, collection);
            }
        }

        private void CreateGitHubListenerForGuild(SocketGuild guild)
        {
            var guildEntity = _databaseHandler.Get<GuildEntity>(guild.Id);
            CreateGitHubListenerForGuild(guild, guildEntity);
        }

        private void CreateGitHubListenerForGuild(SocketGuild guild, GuildEntity guildEntity)
        {

            if (string.IsNullOrWhiteSpace(guildEntity.GitHubWebHookListenToUrl))   // || guildEntity.GitHubWebHookMessageChannelId == default)
                return;

            var settings = GetWebHookSettings(guild, guildEntity);

            new GitHubListener(guildEntity.GitHubWebHookListenToUrl, settings, (msg, severity, ex) =>
            {
                if (ex != null)
                    settings.ErrorOutputChannel?.SendMessageAsync($"WebHook [{severity}]: {msg}:\n{ex}");
                else
                    settings.ErrorOutputChannel?.SendMessageAsync($"WebHook [{severity}]: {msg}");
            });
        }

        private async Task CheckCaseEntities(SocketGuild guild, LiteCollection<CaseEntity> collection)
        {
            var caseEntites = collection.Find(entity => entity.GuildId == guild.Id);
            foreach (var entity in caseEntites)
            {
                switch (entity.CaseType)
                {
                    case CaseType.Ban:
                        try
                        {
                            var ban = await guild.GetBanAsync(entity.UserId);
                            if (ban == null)
                                throw new Exception();
                        }
                        catch
                        {
                            await guild.AddBanAsync(entity.UserId, 0, entity.Reason);
                        }
                        break;
                }
            }
        }

        private GuildWebHookSettings GetWebHookSettings(SocketGuild guild, GuildEntity guildEntity)
        {
            return new GuildWebHookSettings
            {
                Guild = guild,

                #nullable enable
                OutputChannel = new Dictionary<PostType, ITextChannel?> {

                    [PostType.Push] = guildEntity.GitHubWebHookPushChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookPushChannelId) : null,
                    [PostType.IssueOpened] = guildEntity.GitHubWebHookIssueOpenedChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookIssueOpenedChannelId) : null,
                    [PostType.IssueClosed] = guildEntity.GitHubWebHookIssueClosedChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookIssueClosedChannelId) : null,
                    [PostType.IssueCommented] = guildEntity.GitHubWebHookIssueCommentChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookIssueCommentChannelId) : null,
                    [PostType.IssueInitialCommentEdited] = guildEntity.GitHubWebHookIssueInitialCommentEditedChannelId != default
                                                        ? guild.GetTextChannel(guildEntity.GitHubWebHookIssueInitialCommentEditedChannelId) : null,
                    [PostType.IssueNeedTestingAdded] = guildEntity.GitHubWebHookIssueNeedTestingChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookIssueNeedTestingChannelId) : null,
                    [PostType.IssueHelpWantedAdded] = guildEntity.GitHubWebHookIssueHelpWantedChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookIssueHelpWantedChannelId) : null,
                },
                #nullable restore

                ErrorOutputChannel = guildEntity.GitHubWebHookErrorOutputChannelId != default ? guild.GetTextChannel(guildEntity.GitHubWebHookErrorOutputChannelId) : null,

                DeleteNeedTestingAfterLabelRemove = guildEntity.GitHubWebHookIssueNeedTestingDeleteAfterRemove,
                DeleteHelpWantedAfterLabelRemove = guildEntity.GitHubWebHookIssueHelpWantedDeleteAfterRemove,

                BugIssueTitlePrefix = guildEntity.GitHubWebHookIssueBugTitlePrefix,
                SuggestionIssueTitlePrefix = guildEntity.GitHubWebHookIssueSuggestionTitlePrefix,
            };

        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            var guild = _databaseHandler.Get<GuildEntity>(user.Guild.Id);
            var channel = user.Guild.GetTextChannel(guild.WelcomeChannelId);

            if (channel != null && !string.IsNullOrWhiteSpace(guild.WelcomeMessage))
            {
                await channel.SendMessageAsync(guild.WelcomeMessage.Replace(user, user.Guild));
            }

            var mute = _databaseHandler.Get<CaseEntity>($"{user.Id}-Mute");
            if (mute != null)
                _rolesHandler.ChangeRolesToMute(user);
        }

        private async Task OnUserLeft(SocketGuildUser user)
        {
            var guild = _databaseHandler.Get<GuildEntity>(user.Guild.Id);
            var channel = user.Guild.GetTextChannel(guild.UserLeftLogChannelId);

            if (channel != null && !string.IsNullOrWhiteSpace(guild.UserLeftMessage))
            {
                await channel.SendMessageAsync(guild.UserLeftMessage.Replace(user));
            }
        }

        private Task OnDisconnected(Exception _)
        {
            _serviceProvider.GetRequiredService<JobHandler>().CancelJobs();
            return Task.CompletedTask;
        }

        private Task OnLatencyUpdated(int oldPing, int newPing)
        {
            _metricsJob.CollectPing(oldPing, newPing);
            return Task.CompletedTask;
        }

        private async Task OnMessage(SocketMessage socketMessage)
        {
            if (socketMessage.Author.IsBot || socketMessage.Author.IsWebhook)
                return;

            if (!(socketMessage is SocketUserMessage message))
                return;

            var argPos = 1;
            if (!(message.Channel is IPrivateChannel))
            {
                var guildId = message.Channel.CastTo<SocketGuildChannel>().Guild.Id;
                if (await HandleTag(message.Content, guildId, message.Channel))
                    return;

                var guild = _databaseHandler.Get<GuildEntity>(guildId);

                argPos = 0;
                if (!message.HasCharPrefix(guild.Prefix, ref argPos) && !message.HasMentionPrefix(_socketClient.CurrentUser, ref argPos))
                {
                    if (ChannelHelper.IsSupportChannel(guild, message.Channel))
                        await _supportRequestHandler.HandleMessage(guild, message);
                    if (ChannelHelper.IsOnlyCommandsChannel(guild, message.Channel))
                        await message.DeleteAsync();
                    return;
                }
                    
            }            

            var context = new CustomContext(_socketClient, message);
            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider, MultiMatchHandling.Best);

            if (!result.IsSuccess)
            {
                await socketMessage.Author.SendMessageAsync($"{result.Error} error occured. Message:{Environment.NewLine}\"{result.ErrorReason}\"{Environment.NewLine}Used command: \"{message.Content}\"");

                if (message.Channel is SocketGuildChannel channel)
                {
                    var guildId = channel.Guild.Id;
                    var guild = _databaseHandler.Get<GuildEntity>(guildId);
                    if (ChannelHelper.IsOnlyCommandsChannel(guild, message.Channel))
                        await message.DeleteAsync();
                }
            }
                
        }

        private async Task<bool> HandleTag(string msg, ulong guildId, ISocketMessageChannel channel)
        {
            var tagCollection = _databaseHandler.GetCollection<TagEntity>();
            var tag = tagCollection.FindOne(x => x.GuildId == guildId && $"{x.Id}".ToLower() == msg.ToLower());
            if (tag is null)
                return false;
            tag.Uses++;
            await channel.SendMessageAsync(tag.Content);
            tagCollection.Update(tag);

            return true;
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var guildId = channel.CastTo<SocketGuildChannel>().Guild.Id;
            var guild = _databaseHandler.Get<GuildEntity>(guildId);
            await CheckRoleReactionAdded(guild, reaction);
        }

        private async Task CheckRoleReactionAdded(GuildEntity guild, SocketReaction reaction)
        {
            if (guild.UseRolesCommandSystem)
                return;
            if (guild.RolesRequestChannelId != reaction.Channel.Id)
                return;
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot)
                return;
            await _roleReactionHandler.HandleRoleReactionAdded(guild, reaction);
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var guildId = channel.CastTo<SocketGuildChannel>().Guild.Id;
            var guild = _databaseHandler.Get<GuildEntity>(guildId);
            await CheckRoleReactionRemoved(guild, reaction);
        }

        private async Task CheckRoleReactionRemoved(GuildEntity guild, SocketReaction reaction)
        {
            if (guild.UseRolesCommandSystem)
                return;
            if (guild.RolesRequestChannelId != reaction.Channel.Id)
                return;
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot)
                return;
            await _roleReactionHandler.HandleRoleReactionRemoved(guild, reaction);
        }

        private Task OnCommandExecuted(Optional<CommandInfo> optional, ICommandContext basicContext, IResult result)
        {
            (string message, var embed) = result switch
            {
                PreconditionResult preconditionResult
                    => (preconditionResult.ErrorReason, null),

                CustomResult customResult
                    => (customResult.Message, customResult.Embed),
                _
                    => default
            };

            if (string.IsNullOrWhiteSpace(message) && embed is null)
                return Task.CompletedTask;

            var context = basicContext.CastTo<CustomContext>();
            return context.Channel.SendMessageAsync(message, embed: embed);
        }
    }
}
