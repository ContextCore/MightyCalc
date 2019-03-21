using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports.Streams
{
    public class SequencedUsage : FunctionTotalUsage
    {
        public long Sequence { get; set; }
    }
}