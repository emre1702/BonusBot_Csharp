using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TDSConnectorServerAssembly
{
    public class Program
    {
        #nullable disable
        public static DiscordSocketClient DiscordClient { get; set; }
        #nullable restore

        public static void Main() 
        {
            CreateHostBuilder().Build().RunAsync();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostingContext, config) => 
                {
                    config.AddJsonFile("TDSConnectorServerSettings.json", optional: false, reloadOnChange: true);
                });
    }
}
