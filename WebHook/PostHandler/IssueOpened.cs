﻿using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueOpened
    {
        public static EmbedBuilder Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(0, 150, 0)
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Issue.HtmlUrl)
                   .WithDescription(o.Issue.Body)
                   .WithFooter("New issue");

            return builder;
        }
    }
}
