using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports.Streams
{
    public class SequencedFunctionUsage : FunctionTotalUsage
    {
        public long Sequence { get; set; }
    }
}