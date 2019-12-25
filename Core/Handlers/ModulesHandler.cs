using Discord;
using Discord.Commands;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using BonusBot.Entities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BonusBot.Core.Handlers
{
    public sealed class ModulesHandler : IHandler
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly ConcurrentDictionary<string, ContextEntity> _contexts;

        public ModulesHandler(CommandService commandService, IServiceProvider provider)
        {
            _provider = provider;
            _commandService = commandService;
            _contexts = new ConcurrentDictionary<string, ContextEntity>();

            FetchExternalAssemblies();
        }

        private void FetchExternalAssemblies()
        {
            var path = Directory.GetCurrentDirectory();         // Path.Combine(Directory.GetCurrentDirectory(), "Modules");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var files = Directory.EnumerateFiles(path).Where(x => x.Contains("Assembly"));
            foreach (var file in files)
            {
                if (!file.Contains(".dll"))
                    continue;

                var moduleContext = new ModuleLoadContext();
                var ass = moduleContext.LoadFromAssemblyPath(file);
                var name = ass.GetName().Name.SanitizAssembly();

                var context = new ContextEntity
                {
                    Name = name,
                    Context = moduleContext,
                    Assembly = ass
                };

                if (_contexts.ContainsKey(name))
                    continue;

                _contexts.TryAdd(name, context);
                ConsoleHelper.Log(LogSeverity.Info, "Core", $"Loaded {name} v{ass.GetName().Version} assembly.");
            }

            if (!_contexts.IsEmpty)
                return;

            ConsoleHelper.Log(LogSeverity.Warning, "Core", "No external assemblies were found.");
        }

        public async Task TempUnloadAssemblyAsync(string assemblyName)
        {
            if (!_contexts.TryGetValue(assemblyName, out var context))
                return;

            var check = await _commandService.RemoveModuleAsync(context.Module);
            if (!check)
            {
                ConsoleHelper.Log(LogSeverity.Error, "Core", $"Failed to unload {context.Name} module.");
                return;
            }

            context.Context.Unload();
            ConsoleHelper.Log(LogSeverity.Info, "Core", $"Unloaded {context.Name} assembly.");
        }

        public async Task LoadAssemblyAsync(string assemblyName)
        {
            if (!_contexts.TryGetValue(assemblyName, out var context))
                return;

            context.Context.LoadFromAssemblyName(context.Assembly.GetName());
            await _commandService.AddModulesAsync(context.Assembly, _provider);

            ConsoleHelper.Log(LogSeverity.Info, "Core", $"Loaded {context.Name} assembly.");
        }

        public async Task LoadModulesFromAssembliesAsync()
        {
            foreach (var context in _contexts.Values)
            {
                var addModule = await _commandService.AddModulesAsync(context.Assembly, _provider);
                var theModule = addModule.FirstOrDefault();
                if (theModule is null)
                {
                    context.Assembly.EntryPoint.Invoke(null, null);
                    var programClass = context.Assembly.GetTypes().FirstOrDefault(t => t.Name == "Program");
                    programClass?.GetProperty("DiscordClient")?.SetValue(null, _provider.GetRequiredService<DiscordSocketClient>());
                }
                else
                {
                    context.Module = addModule.FirstOrDefault();
                    _contexts.TryUpdate(context.Name, context, context);
                }
            }
        }
    }
}
