using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace TDSConnectorServer
{
    public class TDSServer
    {
#nullable disable
        public static IServiceProvider ServiceProvider { get; set; }
#nullable restore

        public TDSServer()
        {
        }

        public async void Start(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            try
            {
                await CreateHostBuilder().Build().RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureLogging((ILoggingBuilder logging) =>
                {
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .ConfigureKestrel(options =>
                        {
                            options.ListenAnyIP(5000, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                            /*options.Listen(IPAddress.Any, 5000, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http2;
                                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                //listenOptions.UseHttps("/bonusbot-data/TDSConnectorServer.pfx", "tdsv");
                            });*/
                        });
                });

        public static void Main()
        {
        }
    }
}