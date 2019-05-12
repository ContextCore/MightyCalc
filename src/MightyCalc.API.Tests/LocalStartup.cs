using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MightyCalc.Configuration;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Serilog;
using Serilog.Events;

namespace MightyCalc.API.Tests
{
    class LocalStartup : Startup
    {
        public LocalStartup(IConfiguration configuration) : base(configuration)
        {
        }

        private static int counter = 0;
        private ExtendedActorSystem _actorSystem;

        protected override ExtendedActorSystem CreateActorSystem(MightyCalcApiConfiguration cfg)
        {
            _actorSystem = (ExtendedActorSystem) ActorSystem.Create(cfg.ClusterName, cfg.Akka);
            
            var complete = new TaskCompletionSource<bool>();
            var cluster = Cluster.Get(_actorSystem);
            cluster.RegisterOnMemberUp(() => complete.SetResult(true));
            
            if(!complete.Task.Wait(TimeSpan.FromSeconds(10)))
                throw new AkkaClusterIsNotAvailableException();
            
            return _actorSystem;
        }

        protected override DbContextOptions<FunctionUsageContext> GetDbOptions(MightyCalcApiConfiguration cfg)
        {
            return new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase( nameof(UserFunctionLocalTests)+ ++counter).Options;

        }

        protected override MightyCalcApiConfiguration BuildConfiguration()
        {
            var cfg = base.BuildConfiguration();

            var logger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo
                .File($"actor_system_{DateTime.Now:yyyy-MMM-dd-hh-mm-ss}.log").CreateLogger();

            Log.Logger = logger;
            
            cfg.ClusterName = "ApiTest";
            cfg.Akka = ((Config)@"
akka{
    loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
    actor{
        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
        serialize-messages = on 
        serialize-creators = on
        serializers {
            akka-sharding = ""Akka.Cluster.Sharding.Serialization.ClusterShardingMessageSerializer, Akka.Cluster.Sharding""
            hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
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
}").WithFallback(HoconConfigurations.FullDebug);
            return cfg;
        }

    }
}