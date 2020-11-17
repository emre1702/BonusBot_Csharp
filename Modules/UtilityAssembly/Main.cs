using System;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using BonusBot.Common.Helpers;
using Common.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace UtilityAssembly
{
    public sealed partial class UtilityModule : CommandBase
    {
        private readonly DatabaseHandler _databaseHandler;
        private readonly ITDSClient _tdsClient;

        public UtilityModule(DatabaseHandler databaseHandler, ITDSClient tdsClient)
        {
            _databaseHandler = databaseHandler;
            _tdsClient = tdsClient;
        }

        [Command("info")]
        public async Task GetUserInfo(string targetStr)
        {
            var target = GetUser(targetStr);
            if (target == null)
            {
                await ReplyAsync("The user could not be found.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor(target)
                .AddField("Id:", target.Id)
                .AddField("Created:", target.CreatedAt.ToLocalTime(), true)
                .AddField("Joined:", target.JoinedAt.HasValue ? target.JoinedAt.Value.ToLocalTime().ValueToString() : "unknown");
            await ReplyAsync(builder);

            var ban = _databaseHandler.Get<CaseEntity>($"{target.Id}-Ban");
            if (ban != null)
                await ReplyAsync(ban.ToEmbedBuilder(Context.Client));
            var mute = _databaseHandler.Get<CaseEntity>($"{target.Id}-Mute");
            if (mute != null)
                await ReplyAsync(mute.ToEmbedBuilder(Context.Client));
        }

        private SocketGuildUser GetUser(string targetStr)
        {
            SocketGuildUser target = null;
            if (MentionUtils.TryParseUser(targetStr, out ulong userId))
                target = Context.Guild.GetUser(userId);
            if (target != null)
                return target;

            var possibleTargets = Context.Guild.Users.Where(u =>
                u.Id.ToString() == targetStr
                || u.Username.Equals(targetStr, StringComparison.OrdinalIgnoreCase)
                || u.Discriminator.Equals(targetStr, StringComparison.OrdinalIgnoreCase)
                || $"{u.Username}#{u.Discriminator}".Equals(targetStr, StringComparison.OrdinalIgnoreCase));

            if (!possibleTargets.Any())
                return null;

            target = possibleTargets.Where(u => u.Id.ToString() == targetStr).FirstOrDefault();
            if (target != null)
                return target;

            target = possibleTargets.Where(u => $"{u.Username}#{u.Discriminator}".Equals(targetStr, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (target != null)
                return target;

            target = possibleTargets.Where(u => u.Discriminator.Equals(targetStr, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (target != null)
                return target;

            target = possibleTargets.Where(u => u.Username.Equals(targetStr, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return target;
        }
    }
}
