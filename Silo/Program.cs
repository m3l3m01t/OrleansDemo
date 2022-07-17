using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Grains;
using Mono.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Silo
{
  public static class Program
  {
    private const string ConnectionString =
      "Server=127.0.0.1; port=18500; user id=postgres; password=postgres; database=Orleans; pooling=true";

    public static async Task<int> Main(string[] args)
    {
      var dbName = "orleans";
      var dbPasswd = "orleans";
      var dbUser = "orleans";
      var dbHost = "10.106.225.1";
      var dbPort = 5432;

      var nicName = "eth0";
      string address = null;
      var bindAddr = IPAddress.Any;

      var options = new OptionSet
      {
        {"d|device=", "ethernet device", nic => { nicName = nic; }},
        {"b|bind=", "bind on address", addr => address = addr},
        {"h|dhost=", "postgres db server", host => dbHost = host},
        {"p|dport=", "postgres db server port", (int port) => dbPort = port},
        {"db=", "postgres db name", db => dbName = db},
        {"db_passwd=", "postgres db name", passwd => dbPasswd = passwd},
        {"db_user=", "postgres db user", userName => dbUser = userName},
      };

      try
      {
        options.Parse(args);
      }
      catch (OptionException e)
      {
        await Console.Error.WriteLineAsync($"Invalid arguments {e.Message}");

        return -1;
      }

      bindAddr = string.IsNullOrEmpty(address) ? GetPublicAddress(nicName) : IPAddress.Parse(address);

      var builder = new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString)
      {
        Password = dbPasswd,
        Username = dbUser,
        Database = dbName,
        Port = dbPort,
        Host = dbHost
      };

      var connectionString = builder.ToString();

      Console.WriteLine($"Pgsql Connection String: {connectionString}");

      return await RunMainAsync(bindAddr, connectionString, "Npgsql");
    }

    private static IPAddress GetPublicAddress(string nicName)
    {
      var adapters = NetworkInterface.GetAllNetworkInterfaces();
      foreach (var adapter in adapters)
      {
        if (!string.IsNullOrEmpty(nicName) && (adapter.Name != nicName))
        {
          continue;
        }

        if (adapter.NetworkInterfaceType.HasFlag(NetworkInterfaceType.Ethernet) &&
            !adapter.NetworkInterfaceType.HasFlag(NetworkInterfaceType.Loopback) &&
            adapter.Supports(NetworkInterfaceComponent.IPv4) &&
            adapter.OperationalStatus.HasFlag(OperationalStatus.Up))
        {
          var addr = adapter.GetIPProperties().UnicastAddresses.Select(u => u.Address)
            .FirstOrDefault(a =>
              !a.IsIPv6UniqueLocal && (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
          if (addr != null)
          {
            return addr;
          }

          /*
          IPInterfaceProperties properties = adapter.GetIPProperties();
          Console.WriteLine(adapter.Description);
          Console.WriteLine("  DNS suffix .............................. : {0}",
              properties.DnsSuffix);
          Console.WriteLine("  DNS enabled ............................. : {0}",
              properties.IsDnsEnabled);
          Console.WriteLine("  Dynamically configured DNS .............. : {0}",
              properties.IsDynamicDnsEnabled);
          */
        }
      }

      return IPAddress.Loopback;
    }

    private static async Task<int> RunMainAsync(IPAddress address, string connection, string invariant)
    {
      try
      {
        var host = await StartSilo(address, connection, invariant);

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

    private static async Task<ISiloHost> StartSilo(IPAddress advertisedAddr, string connectionString, string invariant)
    {
      // define the cluster configuration
      var builder = new SiloHostBuilder()
        .UseAdoNetClustering(o =>
        {
          o.Invariant = invariant;
          o.ConnectionString = connectionString;
        })
        .UseAdoNetReminderService(o =>
        {
          o.Invariant = invariant;
          o.ConnectionString = connectionString;
        })
        //.AddAdoNetGrainStorage("postgre", o =>
        //{
        //  o.Invariant = invariant;
        //  o.ConnectionString = connectionString;
        //})
        .AddAdoNetGrainStorageAsDefault((AdoNetGrainStorageOptions opt) =>
        {
            opt.Invariant = invariant;
            opt.ConnectionString = connectionString;
            opt.UseJsonFormat = true;
        })
        .Configure<EndpointOptions>(options =>
        {
          //options.SiloPort = 11111;
          options.AdvertisedIPAddress = advertisedAddr;
          options.SiloListeningEndpoint = new System.Net.IPEndPoint(IPAddress.Any, 11111);
          options.GatewayListeningEndpoint = new System.Net.IPEndPoint(IPAddress.Any, 30000);
        })
        .Configure<ClusterOptions>(options =>
        {
          options.ClusterId = "dev";
          options.ServiceId = "OrleansBasics";
        })
        .ConfigureApplicationParts(parts => { parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences(); })
        //.AddLogStorageBasedLogConsistencyProvider("postgre")
        //.AddLogStorageBasedLogConsistencyProviderAsDefault()
        .AddStateStorageBasedLogConsistencyProviderAsDefault()
        .ConfigureLogging(logging => logging.AddConsole());

      var host = builder.Build();
      await host.StartAsync();
      return host;
    }
  }
}

// vim: set ts=2 sw=2 et:
