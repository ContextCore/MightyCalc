using System;

namespace MightyCalc.Reports
{
    public class FunctionUsage
    {
        public string CalculatorName { get; set; }
        public string FunctionName { get; set; }
        public int InvocationsCount { get; set; }
    }
    
    public class TotalFunctionUsage
    {
        public string FunctionName { get; set; }
        public int InvocationsCount { get; set; }
    }
}