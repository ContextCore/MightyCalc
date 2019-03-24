using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports
{
    public interface IReportingDependencies
    {
        FunctionUsageContext CreateFunctionUsageContext();
        IFindProjectionQuery CreateFindProjectionQuery(FunctionUsageContext context = null);
    }
}