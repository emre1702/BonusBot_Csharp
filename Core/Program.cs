using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using BonusBot.Common.Defaults;
using BonusBot.Common.Handlers;
using BonusBot.Common.Helpers;
using BonusBot.Common.Interfaces;
using BonusBot.Core.Handlers;
using Common.Handlers;
using Common.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using InfluxDB.Collector;
using Microsoft.Extensions.DependencyInjection;
using TDSConnectorClient;
using Victoria;

namespace BonusBot.Core
{
    public sealed class Program
    {
        private Assembly _assembly;
        private IServiceProvider _provider;
        private DiscordSocketClient _socketClient;
        private SettingsHandler _settingsHandler;

        public static Task Main()
        {
            return new Program().InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _assembly = Assembly.GetExecutingAssembly();
            _socketClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
            });
            _socketClient.Ready += SocketClient_Ready;

            //_provider.GetRequiredService<DatabaseHandler>().BuildConfiguration(out _config);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE");

            ConsoleHelper.PrintHeader();
            Metrics.Collector = new CollectorConfiguration()
                .Batch.AtInterval(TimeSpan.FromSeconds(5))
                .WriteTo.InfluxDB("http://127.0.0.1:8086", nameof(BonusBot))
                .CreateCollector();

            _settingsHandler = new SettingsHandler();
            var botToken = _settingsHandler.Get<string>(SettingsDefault.Token);
            ConsoleHelper.Log(LogSeverity.Debug, "Core", "Using token for bot: " + botToken);
            await _socketClient.LoginAsync(TokenType.Bot, botToken);
            await _socketClient.StartAsync();

            await Task.Delay(-1);
        }

        private async Task SocketClient_Ready()
        {
            var command = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Warning
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
                    typeof(RoleReactionHandler),
                    typeof(SupportRequestHandler))
                .AddSingleton(command)
                .AddSingleton(_socketClient)
                .AddSingleton(_settingsHandler)
                .AddSingleton<ITDSClient, TDSClient>()
                .BuildServiceProvider();

            await _provider.GetRequiredService<ModulesHandler>().LoadModulesFromAssembliesAsync();
            await _provider.GetRequiredService<CommandService>().AddModulesAsync(_assembly, _provider);

            _provider.GetRequiredService<EventsHandler>();
        }
    }
}
