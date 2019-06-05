using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using InfluxDB.Collector;
using Microsoft.Extensions.DependencyInjection;
using BonusBot.Core.Handlers;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using Victoria;
using BonusBot.Common.Handlers;
using BonusBot.Common.Defaults;
using System.Globalization;

namespace BonusBot.Core
{
    public sealed class Program
    {
        private readonly Assembly _assembly;
        private readonly IServiceProvider _provider;

        private Program()
        {
            _assembly = Assembly.GetExecutingAssembly();
            var command = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Warning
            });
            var socketClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true
            });
            _provider = new ServiceCollection()
                .AddImplementedInterfaces(_assembly, typeof(IJob), typeof(IHandler))
                .AddServices(typeof(LavaSocketClient),
                    typeof(LavaRestClient),
                    typeof(SettingsHandler),
                    typeof(DatabaseHandler),
                    typeof(TrackHandler),
                    typeof(NewPlayerHandler),
                    typeof(AudioInfoHandler),
                    typeof(RolesHandler),
                    typeof(RoleReactionHandler))
                .AddSingleton(command)
                .AddSingleton(socketClient)
                .BuildServiceProvider();

            //_provider.GetRequiredService<DatabaseHandler>().BuildConfiguration(out _config);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE");
        }

        public static Task Main()
        {
            return new Program().InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            ConsoleHelper.PrintHeader();
            Metrics.Collector = new CollectorConfiguration()
                .Batch.AtInterval(TimeSpan.FromSeconds(5))
                .WriteTo.InfluxDB("http://127.0.0.1:8086", nameof(BonusBot))
                .CreateCollector();

            await _provider.GetRequiredService<ModulesHandler>().LoadModulesFromAssembliesAsync();
            await _provider.GetRequiredService<CommandService>().AddModulesAsync(_assembly, _provider);

            var socketClient = _provider.GetRequiredService<DiscordSocketClient>();
            var settingsHandler = _provider.GetRequiredService<SettingsHandler>();
            await socketClient.LoginAsync(TokenType.Bot, settingsHandler.Get<string>(SettingsDefault.Token));
            await socketClient.StartAsync();

            _provider.GetRequiredService<EventsHandler>();
            await Task.Delay(-1);
        }
    }
}
