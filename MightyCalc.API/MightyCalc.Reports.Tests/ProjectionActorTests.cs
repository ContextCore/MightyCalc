using System;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Autofac;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Node;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Reports.Tests
{
    public class ProjectionActorTests:TestKit
    {
        public ProjectionActorTests(ITestOutputHelper output):base("",output)
        {
            var container = new ContainerBuilder();
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(ProjectionActorTests)).Options;

            container.RegisterInstance<IReportingDependencies>(new ReportingDependencies(options));

            Sys.InitReportingExtension(container.Build());
        }



        [Fact]
        public void Given_existing_projection_When_projecting_Then_projection_is_updated()
        {
                throw new NotImplementedException();
        }
        
        [Fact]
        public void Given_not_existing_projection_When_projecting_Then_projection_is_created()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Given_actor_When_sending_function_usages_to_it_Then_they_are_projected()
        {
            var actor = Sys.ActorOf(Props.Create<OverallFunctionUsageProjector>("testEvent"));
            
            actor.Tell(OverallFunctionUsageProjector.Start.Instance);
            var usage = new SequencedUsage
            {
                FunctionName = "testFunction",
                InvocationsCount = 10,
                Sequence = 200
            };
            actor.Tell(usage);

            var dependencies = Sys.GetReportingExtension().GetDependencies();
            var query = dependencies.CreateFindProjectionQuery();
            var projection = query.Execute(KnownProjections.TotalFunctionUsage,
                nameof(OverallFunctionUsageProjector),
                nameof(CalculatorActor.CalculationPerformed));
            
            Assert.Equal(usage.Sequence, projection.Sequence);
           // Assert.Equal(usage.FunctionName, projection.);
            Assert.Equal(usage.Sequence, projection.Sequence);
            

        }
        
        [Fact]
        public void Given_actor_When_sending_function_usages_to_it_And_they_are_projected_Then_next_message_is_received()
        {
            var actor = Sys.ActorOf(Props.Create<OverallFunctionUsageProjector>("testEvent"));
            actor.Tell(OverallFunctionUsageProjector.Start.Instance);
            actor.Tell(new SequencedUsage());
            ExpectMsg<OverallFunctionUsageProjector.Next>();
        }
    }
}