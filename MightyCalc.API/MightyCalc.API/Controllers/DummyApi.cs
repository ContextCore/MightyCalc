using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MightyCalc.API
{
    class DummyApi : IApiController
    {
        public Task<double> CalculateAsync(Expression body)
        {
            return Task.FromResult(0D);
        }

        public Task<IReadOnlyCollection<NamedExpression>> FindFunctionsAsync(string name)
        {
            return Task.FromResult((IReadOnlyCollection<NamedExpression>)new[]
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