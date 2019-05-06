using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Calculations;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class KnownFunctionsQuery : IKnownFunctionsQuery
    {
        private readonly FunctionUsageContext _context;

        public KnownFunctionsQuery(FunctionUsageContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<FunctionDefinition>> Execute(string calculatorId = null,
            string functionName = null)
        {
            IQueryable<KnownFunction> functions = _context.KnownFunctions;
            if (calculatorId != null)
            {
                functions = functions.Where(f => f.CalculatorId == calculatorId);
            }

            if (functionName != null)
            {
                functions = functions.Where(f => f.Name.Contains(functionName));
            }

            var dbRecords = await functions.ToArrayAsync();
            return dbRecords.Select(r =>
                    new FunctionDefinition(r.Name,
                        r.Arity,
                        r.Description,
                        r.Expression,
                        r.Parameters.Split(' ', ',', ';')))
                .ToArray();
        }
    }
}