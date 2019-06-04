using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("disconnect"), AudioProviso]
        [Alias("leave")]
        public async Task DisconnectAsync()
        {
            await _lavaSocketClient.DisconnectAsync(player.VoiceChannel);
            await ReplyAsync("Disconnected!");
        }
    }
}
