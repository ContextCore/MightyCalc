using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MightyCalc.Node;

namespace MightyCalc.API
{
    class AkkaApi : IApiController
    {
        private readonly INamedCalculatorPool _pool;

        public AkkaApi(INamedCalculatorPool pool)
        {
            _pool = pool;
        }

        public Task<double> CalculateAsync(Expression body)
        {
            return _pool.For("anonymous").Calculate(body.Representation,
                body.Parameters.Select(p => new Calculations.Parameter(p.Name, p.Value)).ToArray());
        }

        public async Task<IReadOnlyCollection<NamedExpression>> FindFunctionsAsync(string name)
        {
            var definitions = await _pool.For("anonymous").GetKnownFunction(name);
            return definitions.Select(d => new NamedExpression()
            {
                Description = d.Description,
                Expression = new Expression{Parameters = d.Parameters.Select(p => new Parameter()
                                                        {
                                                            Name = p
                                                        }).ToList(), 
                                             Representation = d.Expression},
                Name = d.Name
            }).ToArray();
        }

        public async Task CreateFunctionAsync(NamedExpression body)
        {
            //API-specific restriction, not coming from business logic! 
            var functionDefinitions = await _pool.For("anonymous").GetKnownFunction(body.Name);
            if(functionDefinitions.Any(f => f.Name == body.Name))
                throw new FunctionAlreadyExistsException();

            await _pool.For("anonymous").AddFunction(body.Name,
                                                            body.Description,
                                                            body.Expression.Representation,
                                                            body.Expression.Parameters.Select(p => p.Name).ToArray());
        }

        public Task ReplaceFunctionAsync(NamedExpression body)
        {
            return _pool.For("anonymous").AddFunction(body.Name,
                body.Description,
                body.Expression.Representation,
                body.Expression.Parameters.Select(p => p.Name).ToArray());
        }

        public Task<Report> UsageStatsAsync(DateTimeOffset? @from, DateTimeOffset? to)
        {
            return Task.FromResult(new Report()
                {UsageStatistics = new List<FunctionUsage>() {new FunctionUsage {Name = "Test", UsageCount = 1}}});
        }
    }
}