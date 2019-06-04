using System.Collections.Generic;
using BonusBot.Entities;
using Discord;
using Victoria.Entities;

namespace BonusBot.Helpers
{
    public sealed class EmbedHelper
    {
        public static EmbedBuilder DefaultEmbed
            => new EmbedBuilder()
            .WithCurrentTimestamp();

        public static EmbedBuilder WithAuthor
            => DefaultEmbed
            .WithColor(171, 31, 242)
            .WithAuthor("BonusBot", "", "https://github.com/emre1702/BonusBot");

        public static EmbedBuilder DefaultAudioInfo
            => new EmbedBuilder()
            .WithColor(0, 0, 150)
            .WithFooter("Audio-info")
            .WithFields(new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder().WithIsInline(true).WithName("Status:").WithValue("playing"),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Volume:"),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Length:"),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Added:"),
                new EmbedFieldBuilder().WithIsInline(false).WithName("Queue:")
            })
            .WithCurrentTimestamp();
    }
}
