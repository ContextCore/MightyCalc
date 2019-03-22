using System;
using System.IO;
using System.Threading.Tasks;
using Akka;
using Akka.IO;
using Akka.Persistence.Query;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.IO;
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
    public class FunctionUsageFlowTests:TestKit
    {
        public FunctionUsageFlowTests(ITestOutputHelper output):base("", output)
        {
            
        }
        
        [Fact]
        public async Task Given_flow_When_pushing_eventEnvelops_to_it_Then_functions_should_be_counted()
        {
            var flow = FunctionTotalUsageFlow.Instance;
            var source = Source.From(new[]
            {
                new EventEnvelope(Offset.Sequence(1),"calcA",1,
                    new CalculatorActor.CalculationPerformed("calcA","a+b+c+d",null, new[]{"add","add","add","add"})),
                new EventEnvelope(Offset.Sequence(2),"calcB",1,
                    new CalculatorActor.CalculationPerformed("calcB","a-b+c",null, new[]{"sub","add"})),
            });

            var sink = Sink.Seq<SequencedFunctionUsage>();

            var runTask = source.RunWith(flow.ToMaterialized(sink, Keep.Right), Sys.Materializer());

            var result = await runTask;

            result.Should().BeEquivalentTo(
                
                new SequencedFunctionUsage {FunctionName = "add", InvocationsCount = 4, Sequence = 1},
                new SequencedFunctionUsage {FunctionName = "sub", InvocationsCount = 1, Sequence = 2},
                new SequencedFunctionUsage {FunctionName = "add", InvocationsCount = 1, Sequence = 2}
            );
        }
    }
}