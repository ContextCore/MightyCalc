using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Node;
using MightyCalc.Node.Akka;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using MightyCalc.Reports.Streams.Projectors;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Reports.Tests
{
    public class FunctionUsageActorTests : TestKit
    {
        public FunctionUsageActorTests(ITestOutputHelper output) : base("", output)
        {
        }

        [Fact]
        public async Task Given_existing_projection_When_projecting_Then_projection_is_updated_And_it_has_data()
        {
            var dependencies = Init(nameof(FunctionUsageActorTests) +
                                    nameof(
                                        Given_existing_projection_When_projecting_Then_projection_is_updated_And_it_has_data
                                    ));
            var context = dependencies.CreateFunctionUsageContext();

            var eventName = "testEvent";
            context.Projections.Add(new Projection
            {
                Event = eventName,
                Name = KnownProjectionsNames.FunctionUsage, Sequence = 11,
                Projector = nameof(FunctionsUsageProjector)
            });

            context.FunctionsUsage.Add(new FunctionUsage
            {
                FunctionName = "testFunction",
                InvocationsCount = 2,
                CalculatorName = "calcA",
                Period = TimeSpan.FromMinutes(1),
                PeriodEnd = DateTimeOffset.Now,
                PeriodStart = DateTimeOffset.Now - TimeSpan.FromMinutes(1)
            });


            var actor = Sys.ActorOf(Props.Create<FunctionsUsageProjector>(eventName, null));

            actor.Tell(ProjectorActorProtocol.Start.Instance);
            ExpectMsg<ProjectorActorProtocol.Next>();

            var usage = new Sequenced<CalculatorActor.CalculationPerformed>()
            {
                Message = new CalculatorActor.CalculationPerformed("calcA","123+1",null,new []{"Add"}),
                Sequence = 200,
            };
            actor.Tell(usage);

            ExpectMsg<ProjectorActorProtocol.Next>();

            var query = dependencies.CreateFindProjectionQuery();
            var projection = query.Execute(KnownProjectionsNames.FunctionUsage,
                nameof(FunctionsUsageProjector),
                eventName);

            //we have an existing projection
            Assert.Equal(usage.Sequence, projection.Sequence);

            //and it contains data 
            var usageQuery = new FunctionsUsageQuery(dependencies.CreateFunctionUsageContext());
            var functionUsage = await usageQuery.Execute("calcA", periodStart: usage.Message.Occured - TimeSpan.FromMinutes(2));

            functionUsage.Should().BeEquivalentTo(new FunctionTotalUsage
                {InvocationsCount = 1, FunctionName = "Add"});
        }

        private IReportingDependencies Init(string dbName)
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(dbName).Options;

            Sys.InitReportingExtension(new ReportingDependencies(options));
            return Sys.GetReportingExtension().GetDependencies();
        }


        [Theory]
        [InlineData("04/19/2019 19:55:41 +08:00", "00:01:00", "04/19/2019 19:55:00 +08:00")]
        [InlineData("04/19/2019 19:55:41 +08:00", "00:00:30", "04/19/2019 19:55:30 +08:00")]
        [InlineData("04/19/2019 19:55:41 +08:00", "00:02:00", "04/19/2019 19:54:00 +08:00")]
        [InlineData("04/19/2019 19:55:41 +08:00", "01:00:00", "04/19/2019 19:00:00 +08:00")]
        public void Given_event_time_When_getting_period_start_Then_it_is_correct(string eventTime, string period,
            string expectedPeriodStart)
        {
            var periodSpan = TimeSpan.Parse(period);
            var eventOffset = DateTimeOffset.Parse(eventTime);
            var expectedPeriodStartOffset = DateTimeOffset.Parse(expectedPeriodStart);
            var periodStart = eventOffset.ToPeriodBegin(periodSpan);
            Assert.Equal(expectedPeriodStartOffset, periodStart);
        }

        [Theory]
        [InlineData("04/19/2019 19:55:41 +08:00", "00:01:00", "04/19/2019 19:56:00 +08:00")]
        [InlineData("04/19/2019 19:55:41 +08:00", "00:00:30", "04/19/2019 19:56:00 +08:00")]
        [InlineData("04/19/2019 19:55:41 +08:00", "00:02:00", "04/19/2019 19:56:00 +08:00")]
        [InlineData("04/19/2019 19:55:41 +08:00", "01:00:00", "04/19/2019 20:00:00 +08:00")]
        public void Given_event_time_When_getting_period_end_Then_it_is_correct(string eventTime, string period,
            string expectedPeriodStart)
        {
            var periodSpan = TimeSpan.Parse(period);
            var eventOffset = DateTimeOffset.Parse(eventTime);
            var expectedPeriodStartOffset = DateTimeOffset.Parse(expectedPeriodStart);
            var periodStart = eventOffset.ToPeriodEnd(periodSpan);
            Assert.Equal(expectedPeriodStartOffset, periodStart);
        }

        [Fact]
        public async Task Given_not_existing_projection_When_projecting_Then_projection_is_created_And_has_data()
        {
            var dependencies = Init(nameof(FunctionUsageActorTests) +
                                    nameof(
                                        Given_not_existing_projection_When_projecting_Then_projection_is_created_And_has_data
                                    ));

            var eventName = "testEvent";

            var actor = Sys.ActorOf(Props.Create<FunctionsUsageProjector>(eventName, null));

            actor.Tell(ProjectorActorProtocol.Start.Instance);
            ExpectMsg<ProjectorActorProtocol.Next>();

            var sequenced = new Sequenced<CalculatorActor.CalculationPerformed>
            {
                Sequence = 200,
                Message = new CalculatorActor.CalculationPerformed("calcA",
                    "123+5", null, new[] {"Add", "Subtract"})
            };

            actor.Tell(sequenced);
            ExpectMsg<ProjectorActorProtocol.Next>();

            var query = dependencies.CreateFindProjectionQuery();
            var projection = query.Execute(KnownProjectionsNames.FunctionUsage,
                nameof(FunctionsUsageProjector),
                eventName);

            //we have an existing projection
            Assert.Equal(sequenced.Sequence, projection.Sequence);

            //and it contains data 
            var usageQuery = new FunctionsUsageQuery(dependencies.CreateFunctionUsageContext());
            var functionUsage = await usageQuery.Execute("calcA");

            functionUsage.Should().BeEquivalentTo(new[]
            {
                new FunctionUsage
                {
                    InvocationsCount = 1,
                    FunctionName = sequenced.Message.FunctionsUsed[0],
                    Period = TimeSpan.FromMinutes(1),
                    CalculatorName = sequenced.Message.CalculatorId,
                    PeriodStart = sequenced.Message.Occured.ToMinutePeriodBegin(),
                    PeriodEnd = sequenced.Message.Occured.ToMinutePeriodEnd()
                },
                new FunctionUsage
                {
                    InvocationsCount = 1,
                    FunctionName = sequenced.Message.FunctionsUsed[1],
                    Period = TimeSpan.FromMinutes(1),
                    CalculatorName = sequenced.Message.CalculatorId,
                    PeriodStart = sequenced.Message.Occured.ToMinutePeriodBegin(),
                    PeriodEnd = sequenced.Message.Occured.ToMinutePeriodEnd()
                }
            });
        }
    }
}