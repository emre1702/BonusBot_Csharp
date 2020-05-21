using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using BonusBot.Common.Entities;
using BonusBot.Common.ExtendedModules;
using BonusBot.Helpers;
using BonusBot.Common.Handlers;
using BonusBot.Common.Attributes;
using LiteDB;

namespace TagAssembly
{
    [RequireContext(ContextType.Guild)]
    [Group("Tag")]
    public sealed class TagModule : CommandBase
    {
        private readonly DatabaseHandler _database;
        private ILiteCollection<TagEntity> _tagsCollection;

        public TagModule(DatabaseHandler databaseHandler)
        {
            _database = databaseHandler;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            _tagsCollection = _database.GetCollection<TagEntity>();

            base.BeforeExecute(command);
        }

        [Command("Add"), Alias("New", "N")]
        [Priority(1)]
        [TagManageProviso]
        public Task Add(string name, [Remainder] string content)
        {
            var prevTag = _tagsCollection.FindOne(x => x.GuildId == Context.Guild.Id && $"{x.Id}".ToLower() == name.ToLower());
            if (prevTag != null)
                return ReplyAsync($"Tag `{prevTag.Id}` already exists. Try another name?");

            var tag = new TagEntity
            {
                Uses = 0,
                Content = content,
                CreatedOn = DateTimeOffset.Now,
                GuildId = Context.Guild.Id,
                Id = name,
                OwnerId = Context.User.Id
            };

            _database.Save(tag);
            return ReplyAsync($"`{name}` tag has been created.");
        }

        [Command("Remove"), Alias("Delete", "Del")]
        [Priority(1)]
        [TagManageProviso]
        public Task Remove([Remainder] string name)
        {
            name = name.Trim('\'', '"');

            var tag = _tagsCollection.FindOne(x => x.GuildId == Context.Guild.Id && $"{x.Id}".ToLower() == name.ToLower());

            if (tag is null)
                return ReplyAsync($"`{name}` tag doesn't exist.");

            _tagsCollection.Delete(new BsonValue(tag.Id));
            return ReplyAsync($"Tag `{name}` has been deleted.");
        }

        [Command("Info")]
        [Priority(1)]
        public async Task<RuntimeResult> Info(string name)
        {
            name = name.Trim('\'', '"');

            var tag = _tagsCollection.FindOne(x => x.GuildId == Context.Guild.Id && $"{x.Id}".ToLower() == name.ToLower());

            if (tag is null)
                return Reply($"`{name}` tag doesn't exist.");

            var user = await Context.GetUserAsync(tag.OwnerId);

            var embed = EmbedHelper.DefaultEmbed
                .WithAuthor($"Tag `{tag.Id}` Owned By {user.Username}", user.GetAvatarUrl())
                .AddField("Tag Content", tag.Content)
                .AddField("Created On", tag.CreatedOn.ToLocalTime())
                .AddField("Total Tag Uses", tag.Uses);

            return Reply(embed);
        }

        [Command("All"), Alias("List")]
        [Priority(1)]
        public Task All()
        {
            var tags = string.Join(", ", _tagsCollection.Find(x => x.GuildId == Context.Guild.Id).Select(x => x.Id));
            return ReplyAsync(tags);
        }
    }
}
