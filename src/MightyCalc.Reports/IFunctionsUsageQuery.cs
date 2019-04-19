using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports
{
    public interface IFunctionsUsageQuery
    {
        Task<IReadOnlyCollection<FunctionUsage>> Execute(string calculatorName=null,  DateTimeOffset? periodStart=null, DateTimeOffset? periodEnd = null);
    }
}