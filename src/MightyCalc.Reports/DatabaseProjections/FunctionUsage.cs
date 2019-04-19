using System;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FunctionUsage
    {
        public string CalculatorName { get; set; }
        public string FunctionName { get; set; }
        public int InvocationsCount { get; set; }
        public DateTimeOffset PeriodStart { get; set; }
        public DateTimeOffset PeriodEnd { get; set; }
        public TimeSpan Period { get; set; }
    }
}