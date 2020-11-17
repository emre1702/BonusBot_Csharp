using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Payloads;
using Victoria.Helpers;

namespace Victoria
{
    public abstract class LavaBaseClient : IAsyncDisposable
    {
        /// <summary>
        /// Spits out important information.
        /// </summary>
        public event Func<LogMessage, Task> OnLog
        {
            add
            {
                Log += value;
            }
            remove
            {
                Log -= value;
            }
        }

        /// <summary>
        /// Fires when Lavalink server sends stats.
        /// </summary>
        public event Func<ServerStats, Task> OnServerStats;

        /// <summary>
        /// Fires when Lavalink server closes connection. 
        /// Params are: <see cref="int"/> ErrorCode, <see cref="string"/> Reason, <see cref="bool"/> ByRemote.
        /// </summary>
        public event Func<int, string, bool, Task> OnSocketClosed;

        public event Func<LavaPlayer, AudioTrack, Task> OnTrackChanged;

        /// <summary>
        /// Fires when a <see cref="LavaTrack"/> is stuck. <see cref="long"/> specifies threshold.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, long, Task> OnTrackStuck;

        /// <summary>
        /// Fires when <see cref="LavaTrack"/> throws an exception. <see cref="string"/> is the error reason.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, string, Task> OnTrackException;

        /// <summary>
        /// Fires when <see cref="AudioTrack"/> receives an updated.
        /// </summary>
        public event Func<LavaPlayer, AudioTrack, TimeSpan, Task> OnPlayerUpdated;

        /// <summary>
        /// Fires when a track has finished playing.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, TrackEndReason, Task> OnTrackFinished;

        /// <summary>
        /// Keeps up to date with <see cref="OnServerStats"/>.
        /// </summary>
        public ServerStats ServerStats { get; private set; }
        public Func<LogMessage, Task> Log { get; set; }

        private BaseSocketClient _baseSocketClient;
        private SocketHelper _socketHelper;
        private Task disconnectTask;
        private CancellationTokenSource _cancellationTokenSource;
        
        protected Configuration Configuration { get; set; }
        protected ConcurrentDictionary<ulong, LavaPlayer> Players { get; set; }

        protected async Task InitializeAsync(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            this._baseSocketClient = baseSocketClient;
            var shards = baseSocketClient switch
            {
                DiscordSocketClient socketClient
                    => await socketClient.GetRecommendedShardCountAsync(),

                DiscordShardedClient shardedClient
                    => shardedClient.Shards.Count,

                _ => 1
            };

            this.Configuration = configuration.SetInternals(baseSocketClient.CurrentUser.Id, shards);
            Players = new ConcurrentDictionary<ulong, LavaPlayer>();
            _cancellationTokenSource = new CancellationTokenSource();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            _socketHelper = new SocketHelper(configuration, Log);
            _socketHelper.OnMessage += OnMessage;
            _socketHelper.OnClosed += OnClosedAsync;

            await _socketHelper.ConnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Connects to <paramref name="voiceChannel"/> and returns a <see cref="LavaPlayer"/>.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <param name="textChannel">Optional text channel that can send updates.</param>
        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = null)
        {
            if (Players.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            await voiceChannel.ConnectAsync(Configuration.SelfDeaf, false, true).ConfigureAwait(false);
            player = new LavaPlayer(voiceChannel, textChannel, _socketHelper);
            Players.TryAdd(voiceChannel.GuildId, player);

            player.OnTrackChanged += (track) =>
            {
                return OnTrackChanged?.Invoke(player, track);
            };

            return player;
        }

        /// <summary>
        /// Disconnects from the <paramref name="voiceChannel"/>.
        /// </summary>
        /// <param name="voiceChannel">Connected voice channel.</param>
        public async Task DisconnectAsync(IVoiceChannel voiceChannel)
        {
            if (!Players.TryRemove(voiceChannel.GuildId, out LavaPlayer player))
                return;

            await player.DisposeAsync();
            await voiceChannel.DisconnectAsync();
            var destroyPayload = new DestroyPayload(voiceChannel.GuildId);
            await _socketHelper.SendPayloadAsync(destroyPayload);
        }

        /// <summary>
        /// Moves voice channels and updates <see cref="LavaPlayer.VoiceChannel"/>.
        /// </summary>
        /// <param name="voiceChannel"><see cref="IVoiceChannel"/></param>
        public async Task MoveChannelsAsync(IVoiceChannel voiceChannel)
        {
            if (!Players.TryGetValue(voiceChannel.GuildId, out var player))
                return;

            if (player.VoiceChannel.Id == voiceChannel.Id)
                return;

            await player.PauseAsync();
            await player.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            await voiceChannel.ConnectAsync(Configuration.SelfDeaf, false, true).ConfigureAwait(false);
            await player.ResumeAsync();

            player.VoiceChannel = voiceChannel;
        }

        /// <summary>
        /// Update the <see cref="LavaPlayer.TextChannel"/>.
        /// </summary>
        /// <param name="channel"><see cref="ITextChannel"/></param>
        public void UpdateTextChannel(ulong guildId, ITextChannel textChannel)
        {
            if (!Players.TryGetValue(guildId, out var player))
                return;

            player.TextChannel = textChannel;
        }

        /// <summary>
        /// Gets an existing <see cref="LavaPlayer"/> otherwise null.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns><see cref="LavaPlayer"/></returns>
        public LavaPlayer GetPlayer(ulong guildId)
        {
            return Players.TryGetValue(guildId, out var player)
                ? player : default;
        }

        public void ToggleAutoDisconnect()
        {
            Configuration.AutoDisconnect = !Configuration.AutoDisconnect;
        }

        private async Task OnClosedAsync()
        {
            if (Configuration.PreservePlayers)
                return;

            foreach (var player in Players.Values)
            {
                await DisconnectAsync(player.VoiceChannel).
                    ContinueWith(_ => player.DisposeAsync());
            }

            Players.Clear();
            Log?.WriteLog(LogSeverity.Warning, "Lavalink died. Disposed all players.");
        }

        private bool OnMessage(string message)
        {
            Log?.WriteLog(LogSeverity.Debug, message);
            var json = JObject.Parse(message);

            var guildId = (ulong)0;
            var player = default(LavaPlayer);

            if (json.TryGetValue("guildId", out var guildToken))
                guildId = ulong.Parse($"{guildToken}");

            var opCode = $"{json.GetValue("op")}";
            switch (opCode)
            {
                case "playerUpdate":
                    if (!Players.TryGetValue(guildId, out player))
                        return false;

                    var state = json.GetValue("state").ToObject<PlayerState>();
                    player.CurrentTrack.Audio.Position = state.Position;
                    player.LastUpdate = state.Time;

                    OnPlayerUpdated?.Invoke(player, player.CurrentTrack, state.Position);
                    break;

                case "stats":
                    ServerStats = json.ToObject<ServerStats>();
                    OnServerStats?.Invoke(ServerStats);
                    break;

                case "event":
                    var evt = json.GetValue("type").ToObject<EventType>();
                    if (!Players.TryGetValue(guildId, out player))
                        return false;

                    var audio = default(LavaTrack);
                    if (json.TryGetValue("track", out var hash))
                        audio = TrackHelper.DecodeTrack($"{hash}");

                    switch (evt)
                    {
                        case EventType.TrackEnd:
                            var endReason = json.GetValue("reason").ToObject<TrackEndReason>();
                            if (endReason != TrackEndReason.Replaced)
                            {
                                if (player.Status != PlayerStatus.Stopped)
                                    player.Status = PlayerStatus.Ended;
                                player.CurrentTrack = default;
                            }
                            OnTrackFinished?.Invoke(player, audio, endReason);
                            break;

                        case EventType.TrackException:
                            var error = json.GetValue("error").ToObject<string>();
                            OnTrackException?.Invoke(player, audio, error);
                            break;

                        case EventType.TrackStuck:
                            var timeout = json.GetValue("thresholdMs").ToObject<long>();
                            OnTrackStuck?.Invoke(player, audio, timeout);
                            break;

                        case EventType.WebSocketClosed:
                            var reason = json.GetValue("reason").ToObject<string>();
                            var code = json.GetValue("code").ToObject<int>();
                            var byRemote = json.GetValue("byRemote").ToObject<bool>();
                            OnSocketClosed?.Invoke(code, reason, byRemote);
                            break;

                        default:
                            Log?.WriteLog(LogSeverity.Warning, $"Missing implementation of {evt} event.");
                            break;
                    }
                    break;

                default:
                    Log?.WriteLog(LogSeverity.Warning, $"Missing handling of {opCode} OP code.");
                    break;
            }

            return true;
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var channel = (oldState.VoiceChannel ?? newState.VoiceChannel);
            var guildId = channel.Guild.Id;


            if (Players.TryGetValue(guildId, out var player)
                && user.Id == _baseSocketClient.CurrentUser.Id)
            {
                player.cachedState = newState;
            }

            if (Configuration.AutoDisconnect)
            {
                var users = channel.Users.Count(x => !x.IsBot);

                if (users > 0)
                {
                    if (disconnectTask is null)
                        return Task.CompletedTask;

                    _cancellationTokenSource.Cancel(false);
                    _cancellationTokenSource = new CancellationTokenSource();
                    return Task.CompletedTask;
                }

                if (!(player is null))
                {
                    Log?.WriteLog(LogSeverity.Warning, $"Automatically disconnecting in {Configuration.InactivityTimeout.TotalSeconds} seconds.");
                    disconnectTask = Task.Run(async () =>
                    {
                        await Task.Delay(Configuration.InactivityTimeout).ConfigureAwait(false);
                        if (player.Status == PlayerStatus.Playing)
                            await player.StopAsync().ConfigureAwait(false);
                        await DisconnectAsync(player.VoiceChannel).ConfigureAwait(false);
                    }, _cancellationTokenSource.Token);
                }
            }

            return Task.CompletedTask;
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue || !Players.TryGetValue(server.Guild.Id, out var player))
                return Task.CompletedTask;

            var update = new VoiceServerPayload(server, player.cachedState.VoiceSessionId);
            return _socketHelper.SendPayloadAsync(update);
        }

        public async ValueTask DisposeAsync()
        {
            await (_socketHelper?.DisposeAsync() ?? default).ConfigureAwait(false);
            _cancellationTokenSource?.Dispose();

            foreach (var player in Players.Values)
            {
                await player.DisposeAsync().ConfigureAwait(false);
            }
            Players.Clear();
            Players = null;

            GC.SuppressFinalize(this);
        }
    }
}
