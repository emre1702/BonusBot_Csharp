using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.Helpers;
using BonusBot.Helpers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Victoria;
using Victoria.Entities;

namespace BonusBot.Common.Handlers
{
    public class AudioInfoHandler
    {
        private DatabaseHandler _databaseHandler;
        private ConcurrentDictionary<ulong, SemaphoreSlim> guildLocks = new ConcurrentDictionary<ulong, SemaphoreSlim>();

        public AudioInfoHandler(DatabaseHandler databaseHandler)
        {
            _databaseHandler = databaseHandler;
        }

        public async Task RefreshTrack(LavaPlayer player)
        {
            await DoWithSemaphore(player, async (player) =>
            {
                EmbedInfo embedInfo = await GetAudioInfoEmbedInfo((SocketGuild)player.VoiceChannel.Guild);
                await UpdateEmbed(embedInfo, player, async (info, p) => await GetRefreshedAudioInfo(info.Embed, p.CurrentTrack));
            });
        }

        public async Task RefreshVolume(LavaPlayer player)
        {
            await DoWithSemaphore(player, async (player) =>
            {
                EmbedInfo embedInfo = await GetAudioInfoEmbedInfo((SocketGuild)player.VoiceChannel.Guild);
                await UpdateEmbed(embedInfo, player, (info, p) => GetRefreshedVolume(info.Embed, p.CurrentVolume));
            });
        }

        public async Task RefreshQueue(LavaPlayer player)
        {
            await DoWithSemaphore(player, async (player) =>
            {
                EmbedInfo embedInfo = await GetAudioInfoEmbedInfo((SocketGuild)player.VoiceChannel.Guild);
                await UpdateEmbed(embedInfo, player, (info, p) => GetRefreshedQueue(info.Embed, p.Queue));
            });
        }

        public async Task RefreshStatus(LavaPlayer player)
        {
            await DoWithSemaphore(player, async (player) =>
            {
                EmbedInfo embedInfo = await GetAudioInfoEmbedInfo((SocketGuild)player.VoiceChannel.Guild);
                await UpdateEmbed(embedInfo, player, (info, p) => GetRefreshedStatus(info.Embed, p));
            });
        }

        private async Task DoWithSemaphore(LavaPlayer player, Func<LavaPlayer, Task> toDo)
        {
            var semaphore = guildLocks.GetOrAdd(player.VoiceChannel.Guild.Id, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                await toDo(player);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task UpdateEmbed(EmbedInfo embedInfo, LavaPlayer player, Func<EmbedInfo, LavaPlayer, Embed> embedRefresher)
        {
            if (embedInfo == null)
                return;
            if (embedInfo.Embed == null)
            {
                embedInfo.Embed = await GetNewAudioInfo(player);
                var msg = await embedInfo.Channel.SendMessageAsync(embed: embedInfo.Embed);
            }
            else
            {
                embedInfo.Embed = embedRefresher(embedInfo, player);
                await((RestUserMessage)embedInfo.Message).ModifyAsync(msg => msg.Embed = embedInfo.Embed);
            }
        }

        private async Task UpdateEmbed(EmbedInfo embedInfo, LavaPlayer player, Func<EmbedInfo, LavaPlayer, Task<Embed>> embedRefresher)
        {
            if (embedInfo == null)
                return;
            if (embedInfo.Embed == null)
            {
                embedInfo.Embed = await GetNewAudioInfo(player);
                var msg = await embedInfo.Channel.SendMessageAsync(embed: embedInfo.Embed);
            }
            else
            {
                embedInfo.Embed = await embedRefresher(embedInfo, player);
                await ((RestUserMessage)embedInfo.Message).ModifyAsync(msg => msg.Embed = embedInfo.Embed);
            }
        }

        private async Task<EmbedInfo> GetAudioInfoEmbedInfo(SocketGuild guild)
        {
            SocketTextChannel channel = GetAudioInfoChannel(guild);
            if (channel == null)
                return null;
            var info = new EmbedInfo() { Channel = channel };

            var messages = await info.Channel.GetMessagesAsync(1).FlattenAsync();
            if (messages.Any())
            {
                info.Message = messages.FirstOrDefault();
                info.Embed = (Embed)info.Message.Embeds.FirstOrDefault();
            }

            return info;
        }

        private SocketTextChannel GetAudioInfoChannel(SocketGuild guild)
        {
            var guildEntity = _databaseHandler.Get<GuildEntity>(guild.Id);
            var channel = guild.GetTextChannel(guildEntity.AudioInfoChannelId);
            return channel;
        }

        private static async Task<Embed> GetNewAudioInfo(LavaPlayer player)
        {
            EmbedBuilder builder = EmbedHelper.DefaultAudioInfo;
            await AddAudioTrackInfo(builder, player.CurrentTrack);
            AddQueueInfo(builder, player.Queue);
            AddVolumeInfo(builder, player.CurrentVolume);

            return builder.Build();
        }

        private static async Task<Embed> GetRefreshedAudioInfo(IEmbed previousEmbed, AudioTrack audioTrack)
        {
            EmbedBuilder builder = previousEmbed.ToEmbedBuilder();
            await AddAudioTrackInfo(builder, audioTrack);
            builder.WithCurrentTimestamp();

            return builder.Build();
        }

        private static Embed GetRefreshedVolume(IEmbed previousEmbed, int volume)
        {
            EmbedBuilder builder = previousEmbed.ToEmbedBuilder();
            AddVolumeInfo(builder, volume);
            builder.WithCurrentTimestamp();

            return builder.Build();
        }

        private static Embed GetRefreshedQueue(IEmbed previousEmbed, LavaQueue queue)
        {
            EmbedBuilder builder = previousEmbed.ToEmbedBuilder();
            AddQueueInfo(builder, queue);
            builder.WithCurrentTimestamp();

            return builder.Build();
        }

        private static Embed GetRefreshedStatus(IEmbed previousEmbed, LavaPlayer player)
        {
            EmbedBuilder builder = previousEmbed.ToEmbedBuilder();
            AddStatusInfo(builder, player);
            builder.WithCurrentTimestamp();

            return builder.Build();
        }

        private static async Task AddAudioTrackInfo(EmbedBuilder builder, AudioTrack audioTrack)
        {
            if (audioTrack != null)
            {
                builder.Author = new EmbedAuthorBuilder() { Name = audioTrack.User.Username, IconUrl = audioTrack.User.GetAvatarUrl() };
                builder.Title = audioTrack.Audio.Title;
                builder.Url = audioTrack.Audio.Uri.AbsoluteUri;
                builder.ThumbnailUrl = await audioTrack.Audio.FetchThumbnailAsync();
                builder.Fields[2].Value = audioTrack.Audio.Length.ToString();
                builder.Fields[3].Value = audioTrack.TimeAdded.ValueToString();
            }
            else
            {
                builder.Author = null;
                builder.Title = null;
                builder.Url = null;
                builder.ThumbnailUrl = null;
                builder.Fields[2].Value = "-";
                builder.Fields[3].Value = "-";
            }
        }

        private static void AddQueueInfo(EmbedBuilder builder, LavaQueue queue)
        {
            string queueStr = queue.ToString();
            builder.Fields[4].Value = string.IsNullOrEmpty(queueStr) ? "-" : queueStr;
        }

        private static void AddVolumeInfo(EmbedBuilder builder, int volume)
        {
            builder.Fields[1].Value = volume;
        }

        private static void AddStatusInfo(EmbedBuilder builder, LavaPlayer player)
        {
            builder.Fields[0].Value = player.Status.ToString();
        }

        private class EmbedInfo
        {
            public SocketTextChannel Channel;
            public Embed Embed;
            public IMessage Message;
        }
    }
}
