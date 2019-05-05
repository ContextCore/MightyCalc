using System;
using Akka;
using Akka.Persistence.Query;
using Akka.Streams.Dsl;
using MightyCalc.Node;
using MightyCalc.Node.Akka;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionUsageFlow
    {

        private static Flow<EventEnvelope, Sequenced<CalculatorActor.CalculationPerformed>, NotUsed> CreateFlow()
        {
            return Flow.Create<EventEnvelope>()
                .Select(e =>
                {
                    Console.WriteLine("");
                    var calculationPerformed = (e.Event as CalculatorActor.CalculationPerformed);
                    //transform each element to pair with number of words in it
                    return new Sequenced<CalculatorActor.CalculationPerformed>
                    {
                        Message = calculationPerformed,
                        Sequence = (e.Offset as Sequence).Value,
                    };
                });
        }

        public static Flow<EventEnvelope, Sequenced<CalculatorActor.CalculationPerformed>, NotUsed> Instance { get; } = CreateFlow();

    }
}