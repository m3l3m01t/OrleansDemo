using GrainInterfaces;

using Microsoft.Extensions.Logging;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;
using Orleans.Streams;

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
                        // var content = Console.ReadLine();
                        var content = DateTime.Now.ToLongTimeString();
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
                        await Task.Delay(500);
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
            var client = new ClientBuilder()
                //.UseLocalhostClustering()
                //.UseStaticClustering(new IPEndPoint(IPAddress.Parse("10.106.225.105"), 30000),
                //  new IPEndPoint(IPAddress.Parse("10.106.225.64"), 30000)
                //)
                .UseAdoNetClustering(o=>{
                    o.Invariant = "Npgsql";
                    o.ConnectionString = "server=127.0.0.1; port=5432; user id=orleans; password=daredevor;database=orleans; pooling=true";
                })
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
            /*
            var stream = client.GetStreamProvider("sp").GetStream<string>(Guid.NewGuid(), "sns");
            await stream.SubscribeAsync((s, token) =>
                {
                    return Task.CompletedTask;
                },
                exception =>
                {
                    return Task.CompletedTask;
                },
                () =>
                {
                    return Task.CompletedTask;
                });
            */
            // example of calling grains from the initialized client
            var friend = client.GetGrain<IHello>(0);
            //var response = await friend.SayHello("Good morning, HelloGrain!");
            var response = await friend.SayHello(content);
            Console.WriteLine($"\nFriend said: {response}\n\n");
            
            var simpleton = client.GetGrain<ISimpleton>(0);
            //var response = await friend.SayHello("Good morning, HelloGrain!");
            var r = await simpleton.Greeting(content);
            Console.WriteLine($"\nSimpleton said: {r}\n\n");
        }
    }
}
