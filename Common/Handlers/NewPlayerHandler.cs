using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.Interfaces;
using Discord;
using Discord.WebSocket;
using Victoria;

namespace BonusBot.Common.Handlers
{
    public class NewPlayerHandler : IHandler
    {
        private readonly LavaSocketClient _lavaSocket;
        private readonly DatabaseHandler _databaseHandler;
        private readonly AudioInfoHandler _audioInfoHandler;

        public NewPlayerHandler(LavaSocketClient lavaSocket, DatabaseHandler databaseHandler, AudioInfoHandler audioInfoHandler)
        {
            _lavaSocket = lavaSocket;
            _databaseHandler = databaseHandler;
            _audioInfoHandler = audioInfoHandler;
        }

        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = null)
        {
            LavaPlayer player = await _lavaSocket.ConnectAsync(voiceChannel, textChannel);
            uint volumeShouldBe = _databaseHandler.Get<GuildEntity>(voiceChannel.GuildId).LastPlayerVolume;
            if (volumeShouldBe != player.CurrentVolume)
                await player.SetVolumeAsync((int)volumeShouldBe);
            player.OnTrackChanged += (_) =>
            {
                return _audioInfoHandler.RefreshTrack(player);
            };
            player.OnVolumeChanged += (_) =>
            {
                return _audioInfoHandler.RefreshVolume(player);
            };
            player.OnStatusChanged += (_) =>
            {
                return _audioInfoHandler.RefreshStatus(player);
            };
            player.OnQueueChanged += () =>
            {
                return _audioInfoHandler.RefreshQueue(player);
            };
            return player;
        }
    }
}
