using Grains;

using Microsoft.Extensions.Logging;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

using System;
using System.Net;
using System.Threading.Tasks;

namespace Silo
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await RunMainAsync();
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();

                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }

            return 0;
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
            	//.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
		.Configure<EndpointOptions>(options =>
		{
		    //options.SiloPort = 11111;
		    //options.GatewayPort = 30000;
		    options.AdvertisedIPAddress = IPAddress.Parse("172.22.131.32");
		    options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 30000);
		    options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 11111);
		})
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                })
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences();
                })
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}

// vim: set ts=4 sw=4 et:
