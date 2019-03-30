using Akka.Actor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.API.Tests
{
    class LocalStartup : Startup
    {
        public LocalStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureExtensions(ActorSystem system, MightyCalcApiConfiguration cfg)
        {
              
        }

        private static int counter = 0;
        protected override DbContextOptions<FunctionUsageContext> GetDbOptions(MightyCalcApiConfiguration cfg)
        {
            return new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase( nameof(UserFunctionLocalTests)+ ++counter).Options;

        }

        protected override ExtendedActorSystem CreateActorSystem(MightyCalcApiConfiguration cfg)
        {
            return (ExtendedActorSystem)ActorSystem.Create("Calc",
                @"akka.actor.provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""");
        }
    }
}