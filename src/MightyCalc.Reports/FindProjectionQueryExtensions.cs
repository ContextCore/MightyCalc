using MightyCalc.Node;
using MightyCalc.Reports.Streams;

namespace MightyCalc.Reports
{
    public static class FindProjectionQueryExtensions
    {
        public static Projection ExecuteForFunctionsTotalUsage(this IFindProjectionQuery query)
        {
            return query.Execute(KnownProjectionsNames.TotalFunctionUsage, nameof(FunctionsTotalUsageProjector),
                nameof(CalculatorActor.CalculationPerformed));
        }

        public static Projection ExecuteForFunctionsUsage(this IFindProjectionQuery query)
        {
            return query.Execute(KnownProjectionsNames.FunctionUsage, nameof(FunctionsUsageProjector),
                nameof(CalculatorActor.CalculationPerformed));
        }
    }
}