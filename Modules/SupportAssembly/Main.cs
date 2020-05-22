using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using BonusBot.Helpers;
using Common.Enums;
using Common.Handlers;
using Common.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TDSConnectorClient;

namespace SupportAssembly
{
    [Group("support")]
    [RequireContext(ContextType.Guild)]
    public sealed class SupportModule : CommandBase
    {
        private readonly SupportRequestHandler _supportRequestHandler;
        private readonly DatabaseHandler _databaseHandler;
        private GuildEntity? _guildEntity;

        /* readonly titleMinLength = 10;
    readonly titleMaxLength = 80;
    readonly messageMinLength = 10;
    readonly messageMaxLength = 255;*/

        public SupportModule(SupportRequestHandler supportRequestHandler, DatabaseHandler databaseHandler)
        {
            _supportRequestHandler = supportRequestHandler;
            _databaseHandler = databaseHandler;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            _guildEntity = _databaseHandler.Get<GuildEntity>(Context.Guild.Id);
            base.BeforeExecute(command);
        }

        protected override void AfterExecute(CommandInfo command)
        {
            _databaseHandler.Save(_guildEntity);
            base.AfterExecute(command);
        }

        [Command("question")]
        public async Task CreateQuestionRequest(int atLeastAdminLevel, string title, [Remainder] string text)
            => await CreateRequest(title, text, SupportType.Question, atLeastAdminLevel);

        [Command("help")]
        public async Task CreateHelpRequest(int atLeastAdminLevel, string title, [Remainder] string text)
            => await CreateRequest(title, text, SupportType.Help, atLeastAdminLevel);

        [Command("compliment")]
        public async Task CreateComplimentRequest(int atLeastAdminLevel, string title, [Remainder] string text)
            => await CreateRequest(title, text, SupportType.Compliment, atLeastAdminLevel);

        [Command("complaint")]
        public async Task CreateComplaintRequest(int atLeastAdminLevel, string title, [Remainder] string text)
            => await CreateRequest(title, text, SupportType.Complaint, atLeastAdminLevel);

        private async Task CreateRequest(string title, string text, SupportType supportType, int atLeastAdminLevel)
        {
            await Context.Message.DeleteAsync();

            if (_guildEntity is null)
                return;


            if (title.Length == 0)
            {
                await Context.User.SendMessageAsync($"The title has to contain atleast {_guildEntity.SupportRequestMinTitleLength} characters." +
                    $"\nDid you maybe forget to use quotation marks for the title?");
                return;
            }

            if (title.Length < _guildEntity.SupportRequestMinTitleLength)
            {
                await Context.User.SendMessageAsync($"The title has to contain atleast {_guildEntity.SupportRequestMinTitleLength} characters." +
                    $"\nDid you maybe forget to use quotation marks for the title?");
                return;
            }
            if (title.Length > _guildEntity.SupportRequestMaxTitleLength)
            {
                await Context.User.SendMessageAsync($"The title can be a maximum of {_guildEntity.SupportRequestMaxTitleLength} characters long.");
                return;
            }

            if (title.Length < _guildEntity.SupportRequestMinTextLength)
            {
                await Context.User.SendMessageAsync($"The text has to contain atleast {_guildEntity.SupportRequestMinTextLength} characters.");
                return;
            }
            if (title.Length > _guildEntity.SupportRequestMaxTextLength)
            {
                await Context.User.SendMessageAsync($"The text can be a maximum of {_guildEntity.SupportRequestMaxTextLength} characters long.");
                return;
            }

            await _supportRequestHandler.CreateRequest(Context.Guild, Context.User, Context.User.Nickname, title, text, supportType, atLeastAdminLevel, true);
        }

        [Command("open")]
        public async Task Open()
        {
            if (_guildEntity is null
                || !(Context.Channel is SocketTextChannel channel)
                || _guildEntity.SupportRequestCategoryId == 0
                || channel.CategoryId != _guildEntity.SupportRequestCategoryId
                || channel.Id == _guildEntity.SupportRequestChannelInfoId
                )
            {
                await Context.Message.DeleteAsync();
                return;
            }

            await _supportRequestHandler.ToggleClosedRequest(channel, Context.User, Context.User.Nickname, false, true);
        }

        [Command("close")]
        public async Task Close()
        {
            if (_guildEntity is null
                || !(Context.Channel is SocketTextChannel channel)
                || _guildEntity.SupportRequestCategoryId == 0
                || channel.CategoryId != _guildEntity.SupportRequestCategoryId
                || channel.Id == _guildEntity.SupportRequestChannelInfoId
                )
            {
                await Context.Message.DeleteAsync();
                return;
            }

            await _supportRequestHandler.ToggleClosedRequest(channel, Context.User, Context.User.Nickname, true, true);
        }
    }
}
