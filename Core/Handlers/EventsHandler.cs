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
using System.Threading;

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

        public EventsHandler(IServiceProvider provider,
            DiscordSocketClient socketClient, CommandService commandService, MetricsJob metricsJob, JobHandler jobHandler,
            LavaSocketClient lavaSocketClient, DatabaseHandler databaseHandler, RolesHandler rolesHandler, RoleReactionHandler roleReactionHandler)
        {
            _socketClient = socketClient;
            _commandService = commandService;
            _serviceProvider = provider;
            _metricsJob = metricsJob;
            _jobHandler = jobHandler;
            _databaseHandler = databaseHandler;
            _rolesHandler = rolesHandler;
            _roleReactionHandler = roleReactionHandler;

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

            foreach (var guild in _socketClient.Guilds)
            {
                _roleReactionHandler.InitForGuild(guild);
                CreateGitHubListenerForGuild(guild);
            }
        }

        private void CreateGitHubListenerForGuild(SocketGuild guild)
        {
            var guildEntity = _databaseHandler.Get<GuildEntity>(guild.Id);

            if (string.IsNullOrWhiteSpace(guildEntity.GitHubWebHookListenToUrl) || guildEntity.GitHubWebHookMessageChannelId == default)
                return;

            var outputToChannel = guild.GetTextChannel(guildEntity.GitHubWebHookMessageChannelId);
            if (outputToChannel == default)
                return;

            new GitHubListener(guildEntity.GitHubWebHookListenToUrl, outputToChannel, (msg, severity, ex) =>
            {
                if (ex != null)
                    outputToChannel.SendMessageAsync($"WebHook [{severity.ToString()}]: {msg}:\n{ex}");
                else
                    outputToChannel.SendMessageAsync($"WebHook [{severity.ToString()}]: {msg}");
            });
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
            if (socketMessage.Author.IsBot || socketMessage.Author.IsWebhook ||
                socketMessage.Channel is IPrivateChannel)
                return;

            if (!(socketMessage is SocketUserMessage message))
                return;

            var guildId = message.Channel.CastTo<SocketGuildChannel>().Guild.Id;
            if (await HandleTag(message.Content, guildId, message.Channel))
                return;

            var guild = _databaseHandler.Get<GuildEntity>(guildId);

            var argPos = 0;
            if (!message.HasCharPrefix(guild.Prefix, ref argPos) && !message.HasMentionPrefix(_socketClient.CurrentUser, ref argPos))
                return;

            var context = new CustomContext(_socketClient, message);
            await _commandService.ExecuteAsync(context, argPos, _serviceProvider, MultiMatchHandling.Best);
        }

        private async Task<bool> HandleTag(string msg, ulong guildId, ISocketMessageChannel channel)
        {
            var tagCollection = _databaseHandler.GetCollection<TagEntity>();
            var tag = tagCollection.FindOne(x => x.GuildId == guildId && $"{x.Id}".ToLower() == msg.ToLower());
            if (tag == null)
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
