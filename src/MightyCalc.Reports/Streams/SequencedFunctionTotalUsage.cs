using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports.Streams
{
    public class SequencedFunctionTotalUsage : FunctionTotalUsage
    {
        public long Sequence { get; set; }
    }
}