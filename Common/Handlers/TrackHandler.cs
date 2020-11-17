using BonusBot.Common.Interfaces;
using System.Collections.Generic;
using Victoria.Entities;

namespace BonusBot.Common.Handlers
{
    public sealed class TrackHandler : IHandler
    {
        public List<LavaTrack> LastSearchResult { get; set; }
    }
}
