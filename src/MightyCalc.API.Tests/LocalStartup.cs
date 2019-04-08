using Akka.Actor;
using Akka.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.API.Tests
{
    class LocalStartup : Startup
    {
        public LocalStartup(IConfiguration configuration) : base(configuration)
        {
        }

        private static int counter = 0;
        protected override DbContextOptions<FunctionUsageContext> GetDbOptions(MightyCalcApiConfiguration cfg)
        {
            return new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase( nameof(UserFunctionLocalTests)+ ++counter).Options;

        }

        protected override MightyCalcApiConfiguration BuildConfiguration()
        {
            var cfg = base.BuildConfiguration();
            cfg.ClusterName = "ApiTest";
            cfg.Akka = (Config) @"
akka{
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
}";
            return cfg;
        }

    }
}