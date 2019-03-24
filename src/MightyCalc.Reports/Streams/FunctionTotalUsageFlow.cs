using System;
using System.Linq;
using Akka;
using Akka.Persistence.Query;
using Akka.Streams.Dsl;
using MightyCalc.Node;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionTotalUsageFlow
    {

        private static Flow<EventEnvelope, SequencedFunctionUsage, NotUsed> CreateFlow()
        {
            return (Flow<EventEnvelope, SequencedFunctionUsage, NotUsed>) Flow.Create<EventEnvelope>()
                .SelectMany(e =>
                {
                    Console.WriteLine("");
                    var calculationPerformed = (e.Event as CalculatorActor.CalculationPerformed);
                    //transform each element to pair with number of words in it
                    return calculationPerformed?.FunctionsUsed.GroupBy(f => f)
                        .Select(g => new SequencedFunctionUsage
                        {
                            FunctionName = g.Key,
                            Sequence = (e.Offset as Sequence).Value,
                            InvocationsCount = g.Count()
                        });
                });
            // split the words into separate streams first
            // .GroupBy(10000, u => u.FunctionName)
            // add counting logic to the streams
            //    .Sum((l, r) => new SequencedFunctionUsage
            //    {
            //        FunctionName = l.FunctionName, InvocationsCount = l.InvocationsCount + r.InvocationsCount,
            //        Sequence = Math.Max(l.Sequence, r.Sequence)
            //    })

            // get a stream of word counts
            //  .MergeSubstreams();
        }

        public static Flow<EventEnvelope, SequencedFunctionUsage, NotUsed> Instance { get; } = CreateFlow();

    }
}