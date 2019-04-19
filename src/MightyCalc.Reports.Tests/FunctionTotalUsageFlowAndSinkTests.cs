using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.Query;
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
    public class FunctionTotalUsageFlowAndSinkTests : TestKit
    {
        public FunctionTotalUsageFlowAndSinkTests(ITestOutputHelper output):base("",output)
        {
            
        }
        [Fact]
        public async Task Given_sink_and_flow_When_pushing_events_to_it_Then_they_should_be_projected()
        {
            var dep = Init(nameof(Given_sink_and_flow_When_pushing_events_to_it_Then_they_should_be_projected));

            var source = Source.From(new[]
            {
                new EventEnvelope(Offset.Sequence(1),"calcA",1,
                    new CalculatorActor.CalculationPerformed("calcA","a+b+c+d",null, new[]{"add","add","add","add"})),
                new EventEnvelope(Offset.Sequence(2),"calcB",1,
                    new CalculatorActor.CalculationPerformed("calcB","a-b+c",null, new[]{"sub","add"})),
                new EventEnvelope(Offset.Sequence(3),"calcA",2,
                    new CalculatorActor.CalculationPerformed("calcA","a+b",null, new[]{"add"})),
                new EventEnvelope(Offset.Sequence(4),"calcC",1,
                    new CalculatorActor.CalculationPerformed("calcC","a+b*e",null, new[]{"add","mul"})),
                
            });
            var sink = FunctionTotalUsageSink.Create(Sys, "testEvent");
            var flow = FunctionTotalUsageFlow.Instance;
            
            source.Via(flow).To(sink).Run(Sys.Materializer());

            await Task.Delay(3000);
            
            var projection = dep.CreateFindProjectionQuery().Execute(KnownProjectionsNames.TotalFunctionUsage,
                nameof(FunctionsTotalUsageProjector), "testEvent");

            Assert.NotNull(projection);
            
            var projectedData = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute("");

            projectedData.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "add", InvocationsCount = 7},
                new FunctionTotalUsage {FunctionName = "sub", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "mul", InvocationsCount = 1}
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