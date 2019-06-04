using BonusBot.Common.Interfaces;
using Victoria.Entities;

namespace BonusBot.Common.Handlers
{
    public sealed class TrackHandler : IHandler
    {
        public LavaTrack[] LastSearchResult { get; set; }
    }
}
