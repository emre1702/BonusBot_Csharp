using Discord;
using Discord.Commands;

namespace BonusBot.Common.ExtendedModules
{
    public sealed class CustomResult : RuntimeResult
    {
        public Embed Embed { get; }
        public string Message { get; }        

        public CustomResult(string message, Embed embed) : base(null, null)
        {
            Message = message;
            Embed = embed;
        }
    }
}
