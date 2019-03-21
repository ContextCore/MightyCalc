using System;
using System.Linq;
using Akka;
using Akka.Persistence.Query;
using Akka.Streams.Dsl;
using MightyCalc.Node;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionUsageCountFlow
    {
        public static Flow<EventEnvelope, SequencedUsage, NotUsed> Instance { get; } =
            (Flow<EventEnvelope, SequencedUsage, NotUsed>) Flow.Create<EventEnvelope>()
                .SelectMany(e =>
                {
                    var calculationPerformed = (e.Event as CalculatorActor.CalculationPerformed);
                    //transform each element to pair with number of words in it
                    return calculationPerformed?.FunctionsUsed.Select(f => new SequencedUsage
                    {
                        FunctionName = f,
                        Sequence = e.SequenceNr,
                        InvocationsCount = 1
                    });
                })
                // split the words into separate streams first
                .GroupBy(1000000, u => u.FunctionName)
                // add counting logic to the streams
                .Sum((l, r) => new SequencedUsage
                {
                    FunctionName = l.FunctionName, InvocationsCount = l.InvocationsCount + r.InvocationsCount,
                    Sequence = Math.Max(l.Sequence, r.Sequence)
                })

                // get a stream of word counts
                .MergeSubstreams();
    }
}