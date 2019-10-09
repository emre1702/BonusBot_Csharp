using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Payloads;
using Victoria.Helpers;

namespace Victoria
{
    /// <summary>
    /// Represents a <see cref="IVoiceChannel"/> connection.
    /// </summary>
    public sealed class LavaPlayer
    {
        public PlayerStatus Status
        {
            get => _status;
            internal set
            {
                _status = value;
                OnStatusChanged.Invoke(_status);
            }
        }

        /// <summary>
        /// Current track that is playing.
        /// </summary>
        public AudioTrack CurrentTrack
        {
            get => _currentTrack;
            internal set
            {
                PreviousTrack = _currentTrack;
                _currentTrack = value;
                OnTrackChanged?.Invoke(_currentTrack);
            }
        }

        /// <summary>
        /// Previous finished track.
        /// </summary>
        public AudioTrack PreviousTrack { get; internal set; }

        /// <summary>
        /// Optional text channel.
        /// </summary>
        public ITextChannel TextChannel { get; internal set; }

        /// <summary>
        /// Connected voice channel.
        /// </summary>
        public IVoiceChannel VoiceChannel { get; internal set; }

        /// <summary>
        /// Default queue, takes an object that implements <see cref="AudioTrack"/>.
        /// </summary>
        public LavaQueue Queue { get; private set; }

        /// <summary>
        /// Last time when Lavalink sent an updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        /// Keeps track of volume set by <see cref="SetVolumeAsync(int)"/>;
        /// </summary>
        public int CurrentVolume
        {
            get => _currentVolume;
            private set
            {
                _currentVolume = value;
                OnVolumeChanged?.Invoke(_currentVolume);
            }
        }

        public event Func<AudioTrack, Task> OnTrackChanged;
        public event Func<int, Task> OnVolumeChanged;
        public event Func<PlayerStatus, Task> OnStatusChanged;
        public event Func<Task> OnQueueChanged;

        private PlayerStatus _status = PlayerStatus.Connected;
        private AudioTrack _currentTrack;
        private int _currentVolume;

        private readonly SocketHelper _socketHelper;
        internal SocketVoiceState cachedState;

        private const string InvalidOp
            = "This operation is invalid since player isn't actually playing anything.";

        internal LavaPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel,
            SocketHelper socketHelper)
        {
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            _socketHelper = socketHelper;
            CurrentVolume = 100;
            Queue = new LavaQueue();
            Queue.OnQueueChanged += () =>
            {
                return OnQueueChanged?.Invoke();
            };
        }

        /// <summary>
        /// Plays the specified <paramref name="track"/>.
        /// </summary>
        /// <param name="track"><see cref="AudioTrack"/></param>
        /// <param name="noReplace">If set to true, this operation will be ignored if a track is already playing or paused.</param>
        public Task PlayAsync(AudioTrack track, bool noReplace = false)
        {
            CurrentTrack = track;
            if (Status != PlayerStatus.Paused)
                Status = PlayerStatus.Playing;
            var payload = new PlayPayload(VoiceChannel.GuildId, track.Audio.Hash, noReplace);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Plays the specified <paramref name="track"/>.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="startTime">Optional setting that determines the number of milliseconds to offset the track by.</param>
        /// <param name="stopTime">optional setting that determines at the number of milliseconds at which point the track should stop playing.</param>
        /// <param name="noReplace">If set to true, this operation will be ignored if a track is already playing or paused.</param>
        public Task PlayAsync(AudioTrack track, TimeSpan startTime, TimeSpan stopTime, bool noReplace = false)
        {
            if (startTime.TotalMilliseconds < 0 || stopTime.TotalMilliseconds < 0)
                throw new InvalidOperationException("Start and stop must be greater than 0.");

            if (startTime <= stopTime)
                throw new InvalidOperationException("Stop time must be greater than start time.");

            CurrentTrack = track;
            if (Status != PlayerStatus.Paused)
                Status = PlayerStatus.Playing;
            var payload = new PlayPayload(VoiceChannel.GuildId, track.Audio.Hash, startTime, stopTime, noReplace);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Stops playing the current track and sets <see cref="IsPlaying"/> to false.
        /// </summary>
        public Task StopAsync()
        {
            if (CurrentTrack == null)
                throw new InvalidOperationException(InvalidOp);

            Status = PlayerStatus.Stopped;
            CurrentTrack = null;
            var payload = new StopPayload(VoiceChannel.GuildId);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Resumes if <see cref="IsPaused"/> is set to true.
        /// </summary>
        public Task ResumeAsync()
        {
            //Volatile.Write(ref _isPaused, false);
            if (CurrentTrack != null)
                Status = PlayerStatus.Playing;
            else
                Status = PlayerStatus.Ended;
            var payload = new PausePayload(VoiceChannel.GuildId, false);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Pauses if <see cref="IsPaused"/> is set to false.
        /// </summary>
        public Task PauseAsync()
        {
            //Volatile.Write(ref _isPaused, true);
            Status = PlayerStatus.Paused;
            var payload = new PausePayload(VoiceChannel.GuildId, true);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Replaces the <see cref="CurrentTrack"/> with the next <see cref="AudioTrack"/> from <see cref="Queue"/>.
        /// </summary>
        /// <returns>Returns the skipped <see cref="AudioTrack"/>.</returns>
        public async Task<AudioTrack> SkipAsync()
        {
            if (!Queue.TryDequeue(out var item))
                throw new InvalidOperationException($"There are no more items in {nameof(Queue)}.");

            if (!(item is AudioTrack track))
                throw new InvalidCastException($"Couldn't cast {item.GetType()} to {typeof(AudioTrack)}.");

            var previousTrack = CurrentTrack;
            await PlayAsync(track);
            return previousTrack;
        }

        /// <summary>
        /// Seeks the <see cref="CurrentTrack"/> to specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">Position must be less than <see cref="CurrentTrack"/>'s position.</param>
        public Task SeekAsync(TimeSpan position)
        {
            if (CurrentTrack == null)
                throw new InvalidOperationException(InvalidOp);

            if (position > CurrentTrack.Audio.Length)
                throw new ArgumentOutOfRangeException($"{nameof(position)} is greater than current track's length.");

            var payload = new SeekPayload(VoiceChannel.GuildId, position);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Updates <see cref="LavaPlayer"/> volume and updates <see cref="CurrentVolume"/>.
        /// </summary>
        /// <param name="volume">Volume may range from 0 to 1000. 100 is default.</param>
        public Task SetVolumeAsync(int volume)
        {
            if (volume > 1000)
                throw new ArgumentOutOfRangeException($"{nameof(volume)} was greater than max limit which is 1000.");

            CurrentVolume = volume;
            var payload = new VolumePayload(VoiceChannel.GuildId, volume);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Change the <see cref="LavaPlayer"/>'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands"><see cref="EqualizerBand"/></param>
        public Task EqualizerAsync(List<EqualizerBand> bands)
        {
            if (CurrentTrack == null)
                throw new InvalidOperationException(InvalidOp);

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Change the <see cref="LavaPlayer"/>'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands"><see cref="EqualizerBand"/></param>
        public Task EqualizerAsync(params EqualizerBand[] bands)
        {
            if (CurrentTrack == null)
                throw new InvalidOperationException(InvalidOp);

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            return _socketHelper.SendPayloadAsync(payload);
        }

        internal ValueTask DisposeAsync()
        {
            Status = PlayerStatus.Disconnected;
            Queue.Clear();
            OnQueueChanged?.Invoke();
            Queue = null;
            CurrentTrack = null;
            GC.SuppressFinalize(this);

            return default;
        }
    }
}
