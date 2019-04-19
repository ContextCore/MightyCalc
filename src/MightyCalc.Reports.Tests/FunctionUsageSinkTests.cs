using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.TestKit.Xunit2;
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
    public class FunctionUsageSinkTests : TestKit
    {
        public FunctionUsageSinkTests(ITestOutputHelper output) : base("", output)
        {
        }

        [Fact]
        public async Task Given_sink_When_pushing_events_to_it_Then_they_should_be_projected()
        {
            var dep = Init(nameof(FunctionUsageSinkTests) +
                           nameof(Given_sink_When_pushing_events_to_it_Then_they_should_be_projected));

            var now = DateTimeOffset.Now;
            var source = Source.From(new[]
            {
                new Sequenced<CalculatorActor.CalculationPerformed>
                {
                    Message = new CalculatorActor.CalculationPerformed("CalcA", "1+1", null, new[] {"myFunc"}),
                    Sequence = 13
                },
                new Sequenced<CalculatorActor.CalculationPerformed>
                {
                    Message = new CalculatorActor.CalculationPerformed("CalcA", "1+1", null, new[] {"myFunc"}),
                    Sequence = 14
                },
                new Sequenced<CalculatorActor.CalculationPerformed>
                {
                    Message = new CalculatorActor.CalculationPerformed("CalcA", "1+2", null, new[] {"addition"}),
                    Sequence = 15
                }
            });

            var sink = FunctionUsageSink.Create(Sys, "testEvent");

            source.RunWith(sink, Sys.Materializer());

            await Task.Delay(3000);

            var projection = dep.CreateFindProjectionQuery().Execute(KnownProjectionsNames.FunctionUsage,
                nameof(FunctionsUsageProjector), "testEvent");

            Assert.NotNull(projection);
            var projectedData = await new FunctionsUsageQuery(dep.CreateFunctionUsageContext()).Execute("CalcA");

            projectedData.Should().BeEquivalentTo(
                new FunctionUsage
                {
                    FunctionName = "myFunc", CalculatorName = "CalcA", InvocationsCount = 2,
                    Period = TimeSpan.FromMinutes(1), PeriodEnd = now.ToMinutePeriodEnd(),
                    PeriodStart = now.ToMinutePeriodBegin()
                },
                new FunctionUsage
                {
                    FunctionName = "addition", CalculatorName = "CalcA", InvocationsCount = 1,
                    Period = TimeSpan.FromMinutes(1), PeriodEnd = now.ToMinutePeriodEnd(),
                    PeriodStart = now.ToMinutePeriodBegin()
                }
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