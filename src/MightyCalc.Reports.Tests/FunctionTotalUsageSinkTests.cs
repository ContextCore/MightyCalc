using System.Threading.Tasks;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using MightyCalc.Reports.Streams.Projectors;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Reports.Tests
{
    public class FunctionTotalUsageSinkTests : TestKit
    {
        public FunctionTotalUsageSinkTests(ITestOutputHelper output) : base("", output)
        {
        }

        [Fact]
        public async Task Given_sink_When_pushing_events_to_it_Then_they_should_be_projected()
        {
            var dep = Init(nameof(Given_sink_When_pushing_events_to_it_Then_they_should_be_projected));

            var source = Source.From(new[]
            {
                new SequencedFunctionTotalUsage {FunctionName = "myFunc", InvocationsCount = 5, Sequence = 13},
                new SequencedFunctionTotalUsage {FunctionName = "myFunc", InvocationsCount = 6, Sequence = 14},
                new SequencedFunctionTotalUsage {FunctionName = "addition", InvocationsCount = 1, Sequence = 15},
            });

            var sink = FunctionTotalUsageSink.Create(Sys, "testEvent");

            source.RunWith(sink, Sys.Materializer());

            await Task.Delay(3000);

            var projection = dep.CreateFindProjectionQuery().Execute(KnownProjectionsNames.TotalFunctionUsage,
                nameof(FunctionsTotalUsageProjector), "testEvent");

            Assert.NotNull(projection);
            var projectedData = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute("");

            projectedData.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "myFunc", InvocationsCount = 11},
                new FunctionTotalUsage {FunctionName = "addition", InvocationsCount = 1}
            );
        }

        private IReportingDependencies Init(string dbName)
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options;

            Sys.InitReportingExtension(new ReportingDependencies(options));
            return Sys.GetReportingExtension().GetDependencies();
        }
    }
}