using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Autofac;
using FluentAssertions;
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
           
        }

        [Fact]
        public async Task Given_existing_projection_When_projecting_Then_projection_is_updated_And_it_has_data()
        {
            var dependencies = Init(nameof(Given_existing_projection_When_projecting_Then_projection_is_updated_And_it_has_data));
            var context = dependencies.CreateFunctionUsageContext();

            var eventName = "testEvent";
            context.Projections.Add(new Projection
            {
                Event = eventName, 
                Name = KnownProjectionsNames.TotalFunctionUsage, Sequence = 11,
                Projector = nameof(FunctionsTotalUsageProjector)
            });

            context.FunctionsTotalUsage.Add(new FunctionTotalUsage
            {
                FunctionName = "testFunction",
                InvocationsCount = 2
            });
            
            
            var actor = Sys.ActorOf(Props.Create<FunctionsTotalUsageProjector>(eventName));
            
            actor.Tell(FunctionsTotalUsageProjector.Start.Instance);

            var usage = new SequencedUsage
            {
                FunctionName = "testFunction",
                InvocationsCount = 10,
                Sequence = 200
            };
            actor.Tell(usage);

            ExpectMsg<FunctionsTotalUsageProjector.Next>();
            
            var query = dependencies.CreateFindProjectionQuery();
            var projection = query.Execute(KnownProjectionsNames.TotalFunctionUsage,
                nameof(FunctionsTotalUsageProjector),
                eventName);
            
            //we have an existing projection
            Assert.Equal(usage.Sequence, projection.Sequence);
            
            //and it contains data 
            var usageQuery = new FunctionsTotalUsageQuery(dependencies.CreateFunctionUsageContext());
            var functionUsage = await usageQuery.Execute("test");
            
            functionUsage.Should().BeEquivalentTo(new FunctionTotalUsage{InvocationsCount = usage.InvocationsCount,FunctionName = usage.FunctionName});
        }

        private IReportingDependencies Init(string dbName)
        {
            var container = new ContainerBuilder();
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(dbName).Options;

            container.RegisterInstance<IReportingDependencies>(new ReportingDependencies(options));
            Sys.InitReportingExtension(container.Build());
            return Sys.GetReportingExtension().GetDependencies();
        }

        [Fact]
        public async Task Given_not_existing_projection_When_projecting_Then_projection_is_created_And_has_data()
        {
            var dependencies = Init(nameof(Given_not_existing_projection_When_projecting_Then_projection_is_created_And_has_data));

            var eventName = "testEvent";
            
            var actor = Sys.ActorOf(Props.Create<FunctionsTotalUsageProjector>(eventName));
            
            actor.Tell(FunctionsTotalUsageProjector.Start.Instance);

            var usage = new SequencedUsage
            {
                FunctionName = "testFunction",
                InvocationsCount = 10,
                Sequence = 200
            };
            actor.Tell(usage);
            ExpectMsg<FunctionsTotalUsageProjector.Next>();

            var query = dependencies.CreateFindProjectionQuery();
            var projection = query.Execute(KnownProjectionsNames.TotalFunctionUsage,
                nameof(FunctionsTotalUsageProjector),
                eventName);
            
            //we have an existing projection
            Assert.Equal(usage.Sequence, projection.Sequence);
            
            //and it contains data 
            var usageQuery = new FunctionsTotalUsageQuery(dependencies.CreateFunctionUsageContext());
            var functionUsage = await usageQuery.Execute("test");
            
            functionUsage.Should().BeEquivalentTo(new FunctionTotalUsage{InvocationsCount = usage.InvocationsCount,FunctionName = usage.FunctionName});
        }

        
        [Fact]
        public void Given_actor_When_sending_function_usages_to_it_And_they_are_projected_Then_next_message_is_received()
        {
            Init(nameof(Given_actor_When_sending_function_usages_to_it_And_they_are_projected_Then_next_message_is_received));
            var actor = Sys.ActorOf(Props.Create<FunctionsTotalUsageProjector>("testEvent"));
            actor.Tell(FunctionsTotalUsageProjector.Start.Instance);
            actor.Tell(new SequencedUsage());
            ExpectMsg<FunctionsTotalUsageProjector.Next>();
        }
    }
}