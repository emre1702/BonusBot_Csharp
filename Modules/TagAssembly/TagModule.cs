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

        public TagModule(DatabaseHandler databaseHandler)
        {
            _database = databaseHandler;
        }


        [Command("Add"), Alias("New", "N")]
        [Priority(1)]
        [TagManageProviso]
        public async Task Add(string name, [Remainder] string content)
        {
            var prevTag = await _database.GetCollection<TagEntity>(tagsCollection
                => tagsCollection.FindOne(x => x.GuildId == Context.Guild.Id && $"{x.Id}".ToLower() == name.ToLower()));
            if (prevTag != null)
                await ReplyAsync($"Tag `{prevTag.Id}` already exists. Try another name?");

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

            await ReplyAsync($"`{name}` tag has been created.");
        }

        [Command("Remove"), Alias("Delete", "Del")]
        [Priority(1)]
        [TagManageProviso]
        public async Task Remove([Remainder] string name)
        {
            name = name.Trim('\'', '"');

            var tag = await _database.GetCollection<TagEntity>(tagsCollection
                => tagsCollection.FindOne(x => x.GuildId == Context.Guild.Id && $"{x.Id}".ToLower() == name.ToLower()));
            if (tag is null)
                await ReplyAsync($"`{name}` tag doesn't exist.");

            _database.Delete(tag);
            await ReplyAsync($"Tag `{name}` has been deleted.");
        }

        [Command("Info")]
        [Priority(1)]
        public async Task Info(string name)
        {
            name = name.Trim('\'', '"');

            var tag = await _database.GetCollection<TagEntity>(tagsCollection
                => tagsCollection.FindOne(x => x.GuildId == Context.Guild.Id && $"{x.Id}".ToLower() == name.ToLower()));

            if (tag is null)
                await ReplyAsync($"`{name}` tag doesn't exist.");

            var user = await Context.GetUserAsync(tag.OwnerId);

            var embed = EmbedHelper.DefaultEmbed
                .WithAuthor($"Tag `{tag.Id}` Owned By {user.Username}", user.GetAvatarUrl())
                .AddField("Tag Content", tag.Content)
                .AddField("Created On", tag.CreatedOn.ToLocalTime())
                .AddField("Total Tag Uses", tag.Uses);
            await ReplyAsync(embed);
        }

        [Command("All"), Alias("List")]
        [Priority(1)]
        public async Task All()
        {
            var tags = await _database.GetCollection<TagEntity, string>(tagsCollection
                => string.Join(", ", tagsCollection.Find(x => x.GuildId == Context.Guild.Id).Select(x => x.Id)));
            await ReplyAsync(tags);
        }
    }
}
