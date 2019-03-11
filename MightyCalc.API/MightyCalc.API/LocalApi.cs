using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MightyCalc.Calculations;

namespace MightyCalc.API
{
    class LocalApi : IApiController
    {
        private readonly ICalculator _calculator;

        public LocalApi(ICalculator calculator)
        {
            _calculator = calculator;
        }
        public Task<double> CalculateAsync(Expression body)
        {
            var parameters = body.Parameters.Select(p => new Calculations.Parameter(p.Name, p.Value)).ToArray();
            var result = _calculator.Calculate(body.Representation, parameters);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<NamedExpression>> FindFunctionsAsync(string name)
        {
            return Task.FromResult((IReadOnlyCollection<NamedExpression>) new[]
            {
                new NamedExpression()
                {
                    Expression = new Expression
                    {
                        Representation = "Test(a,b)",
                        Parameters = new List<Parameter>
                        {
                            new Parameter() {Name = "a"},
                            new Parameter() {Name = "b"}
                        }
                    },
                    Name = "Test"
                }
            });
        }

        public Task CreateFunctionAsync(NamedExpression body)
        {
            return Task.CompletedTask;
        }

        public Task ReplaceFunctionAsync(NamedExpression body)
        {
            return Task.CompletedTask;
        }

        public Task DeleteFunctionAsync(string name)
        {
            return Task.CompletedTask;
        }

        public Task<Report> UsageStatsAsync(DateTimeOffset? @from, DateTimeOffset? to)
        {
            return Task.FromResult(new Report()
                {UsageStatistics = new List<FunctionUsage>() {new FunctionUsage {Name = "Test", UsageCount = 1}}});
        }
    }
}