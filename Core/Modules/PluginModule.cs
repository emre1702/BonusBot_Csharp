using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using BonusBot.Common.ExtendedModules;
using BonusBot.Core.Handlers;

namespace BonusBot.Modules
{
    public sealed class PluginModule : CommandBase
    {
        private readonly ModulesHandler _modulesHandler;

        public PluginModule(ModulesHandler modulesHandler)
        {
            _modulesHandler = modulesHandler;
        }

        [Command("Unload", RunMode = RunMode.Async)]
        public async Task Unload(string assemblyName)
        {

            await _modulesHandler.TempUnloadAssemblyAsync(assemblyName);
        }

        [Command("load", RunMode =RunMode.Async)]
        public async Task load(string assemblyName)
        {

            await _modulesHandler.LoadAssemblyAsync(assemblyName);
        }
    }
}
