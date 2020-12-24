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

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
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
                            options.Listen(IPAddress.Loopback, 5000, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http2;
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                    listenOptions.UseHttps("/home/localhost.pfx", "grpc");
                            });
                        });
                });

        public static void Main()
        {
        }
    }
}
