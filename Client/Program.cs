using GrainInterfaces;

using Microsoft.Extensions.Logging;

using Orleans;
using Orleans.Configuration;

using System;
using System.Threading.Tasks;

namespace Client
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
                using (var client = await ConnectClient())
                {
                    while (true)
                    {
                        var content = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            continue;
                        }
                        if (content == "quit")
                        {
                            break;
                        }
                        await DoClientWork(client, content);
                        //Console.ReadKey();
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }

        private static async Task DoClientWork(IClusterClient client, string content)
        {
            // example of calling grains from the initialized client
            var friend = client.GetGrain<IHello>(0);
            //var response = await friend.SayHello("Good morning, HelloGrain!");
            var response = await friend.SayHello(content);
            Console.WriteLine($"\n\n{response}\n\n");
        }
    }
}
