using System.Reflection;
using Discord.Commands;
using BonusBot.Common.ExtendedModules;

namespace BonusBot.Entities
{
    public sealed class ContextEntity
    {
        public string Name { get; set; }
        public ModuleInfo Module { get; set; }
        public Assembly Assembly { get; set; }
        public ModuleLoadContext Context { get; set; }        
    }
}
