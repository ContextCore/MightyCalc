using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Cluster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MightyCalc.Client;
using MightyCalc.Node;
using MightyCalc.Reports;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Swashbuckle.AspNetCore.Swagger;

namespace MightyCalc.API.Tests
{

    public class CalculationLocalTests : CalculationTests
    {
        private string NodeConfig = @"
akka{
 actor.provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
 remote {
          dot-netty.tcp {
              port = 30031
              hostname = localhost
          }
   }
 cluster {
        seed-nodes = [""akka.tcp://MightyCalc@localhost:30031""]
        roles = [calculation]
    }
}
";

        private ActorSystem _node;

        protected override IMightyCalcClient CreateClient()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<LocalStartup>(); 
            
            var server = new TestServer(builder);
            
            _node = ActorSystem.Create("MightyCalc", NodeConfig);
            
            var httpClient = server.CreateClient();
            return new MightyCalcClient("", httpClient);
        }

        protected override void Dispose(bool disposing)
        {
            _node.Dispose();
        }
    }
}