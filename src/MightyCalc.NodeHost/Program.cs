using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Hocon.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightCalc.NodeHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();


            var config = new NodeConfiguration();
            configuration.GetSection("Node")?.Bind(config);

            var hoconPath = Path.Combine(ExecutingAssemblyFolder(), "akka.hocon");
            if (File.Exists(hoconPath))
            {
                Config cfg = File.ReadAllText(hoconPath);
                if(cfg.GetBoolean("enabled"))
                    config.AkkaConfig = cfg;
            }

            var system = ActorSystem.Create(config.ClusterName, config.AkkaConfig);
            var cluster = Cluster.Get(system);
            cluster.RegisterOnMemberUp(() => OnStart(system, config.ReadModel));
            
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void OnStart(ActorSystem system, string readModelConnectionString)
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseNpgsql(readModelConnectionString)
                .Options;

            system.InitReportingExtension(new ReportingDependencies(options)).Start();
        }

        private static string ExecutingAssemblyFolder()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var executingAssemblyFolder = Path.GetDirectoryName(location);
            return executingAssemblyFolder;
        }
    }
}