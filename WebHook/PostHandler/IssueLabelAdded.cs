using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Discord;
using WebHook.Entity.GitHub;
using SysColor = System.Drawing.Color;

namespace WebHook.PostHandler
{
    class IssueLabelAdded
    {
        public static List<EmbedBuilder> Handle(Base o)
        {
            var color = SysColor.FromArgb(int.Parse(o.Label.HexColor, NumberStyles.HexNumber));
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(new Color(color.R, color.G, color.B))
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Issue.HtmlUrl)
                   .WithDescription(Handler.SplitForEmbedDescription(o.Issue.Body).First())
                   .WithFooter(o.Label.Name);

            return new List<EmbedBuilder> { builder };
        }
    }
}
