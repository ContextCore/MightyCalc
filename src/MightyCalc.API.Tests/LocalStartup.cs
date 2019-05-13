using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Serialization;
using GridDomain.Node.Akka.Configuration.Hocon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MightyCalc.Configuration;
using MightyCalc.Node;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Serilog;
using Serilog.Events;

namespace MightyCalc.API.Tests
{

    class DebugHyperionSerializer : HyperionSerializer
    {
        public DebugHyperionSerializer(ExtendedActorSystem system) : base(system)
        {
        }

        public DebugHyperionSerializer(ExtendedActorSystem system, Config config) : base(system, config)
        {
        }

        public DebugHyperionSerializer(ExtendedActorSystem system, HyperionSerializerSettings settings) : base(system, settings)
        {
        }

        public override byte[] ToBinary(object obj)
        {
            try
            {
                return base.ToBinary(obj);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            try
            {
                return base.FromBinary(bytes, type);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
    
    
    class LocalStartup : Startup
    {
        public LocalStartup(IConfiguration configuration) : base(configuration)
        {
        }

        private static int counter = 0;

        protected override void ConfigureExtensions(ActorSystem system, MightyCalcApiConfiguration cfg)
        {
            base.ConfigureExtensions(system, cfg);
             system.InitReportingExtension(new ReportingDependencies(GetDbOptions(cfg))).Start();
        }

        protected override ExtendedActorSystem CreateActorSystem(MightyCalcApiConfiguration cfg)
        {
            return (ExtendedActorSystem)ActorSystem.Create(cfg.ClusterName, cfg.Akka);
        }

        protected override DbContextOptions<FunctionUsageContext> GetDbOptions(MightyCalcApiConfiguration cfg)
        {
            return new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase( nameof(UserFunctionTests)+ ++counter).Options;

        }

        protected override MightyCalcApiConfiguration BuildConfiguration()
        {
            var cfg = base.BuildConfiguration();

            var logger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo
                .File($"actor_system_{DateTime.Now:yyyy-MMM-dd-HH-mm-ss}.log").CreateLogger();

            Log.Logger = logger;

            var a = typeof(DebugHyperionSerializer);
            //  #Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
            cfg.ClusterName = "ApiTest";
            var configString = @"
akka{
    loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
    actor{
        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
        serialize-messages = on 
        serialize-creators = on
        serializers {
            akka-sharding = ""Akka.Cluster.Sharding.Serialization.ClusterShardingMessageSerializer, Akka.Cluster.Sharding""
            hyperion = """ + typeof(DebugHyperionSerializer).AssemblyQualifiedShortName() + @""" 
        }
                        
        serialization-bindings {
            ""Akka.Cluster.Sharding.IClusterShardingSerializable, Akka.Cluster.Sharding"" = akka-sharding
            ""System.Object"" = hyperion
        }
            
        serialization-identifiers {
            ""Akka.Cluster.Sharding.Serialization.ClusterShardingMessageSerializer, Akka.Cluster.Sharding"" = 13
        }
    }
    remote {
          dot-netty.tcp {
              port = 30031
              hostname = localhost
          }
    }
    cluster{
        seed-nodes = [""akka.tcp://ApiTest@localhost:30031""]
        roles = [api, calculation, projection]
    }
}";
            cfg.Akka = ((Config)configString).WithFallback(HoconConfigurations.FullDebug);
            return cfg;
        }

    }
}