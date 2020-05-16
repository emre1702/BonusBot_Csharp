using System.Collections.Generic;
using BonusBot.Entities;
using Common.Enums;
using Discord;
using Discord.WebSocket;
using Victoria.Entities;

namespace BonusBot.Helpers
{
    public sealed class EmbedHelper
    {
        public static EmbedBuilder DefaultEmbed
            => new EmbedBuilder()
            .WithCurrentTimestamp();


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

        public static EmbedBuilder GetSupportRequestEmbed(SocketGuildUser author, string title, string text, SupportType supportType)
            => new EmbedBuilder()
            .WithColor(supportType switch {
                SupportType.Question => new Color(0, 0, 150),
                SupportType.Help => new Color(150, 150, 150),
                SupportType.Compliment => new Color(0, 150, 0),
                SupportType.Complaint => new Color(150, 0, 0),
                _ => new Color(255, 255, 255)
            })
            .WithAuthor(author)
            .WithCurrentTimestamp()
            .WithDescription(text)
            .WithFooter(supportType.ToString())
            .WithTitle(title);
    }
}
