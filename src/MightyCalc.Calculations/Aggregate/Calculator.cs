using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GridDomain.Aggregates;
using MightyCalc.Calculations.Aggregate.Commands;
using MightyCalc.Calculations.Aggregate.Errors;
using MightyCalc.Calculations.Aggregate.Events;

namespace MightyCalc.Calculations.Aggregate
{
    public class Calculator : IAggregate
    {
        private readonly SpracheCalculator _calculatorEngine = new SpracheCalculator();

        private static void AddFunction(SpracheCalculator calculator, FunctionDefinition functionDefinition)
        {
            calculator.AddFunction(functionDefinition.Name,
                functionDefinition.Description,
                functionDefinition.Expression,
                functionDefinition.Parameters);
        }

        public void Apply(IDomainEvent @event)
        {
            switch (@event)
            {
                case CalculationPerformed c: break;
                case CalculatorCreated c:
                    Id = c.Source.Id;
                    break;
                case FunctionAdded a: 
                   AddFunction(_calculatorEngine, a.Definition); break;
            }
            Version++;
        }

        public string Id { get; private set; }
        public int Version { get; private set; }

        public Task<IReadOnlyCollection<IDomainEvent>> Execute(ICommand command)
        {
            switch (command)
            {
                case CreateCalculator c:
                    if (Id != null)
                        throw new CalculatorAlreadyCreatedException();
                    return new CalculatorCreated(c.Recipient.Id).AsCommandResult();
                                    
                case CalculateExpression c: 
                    var result = _calculatorEngine.Calculate(c.Representation, c.Parameters);
                    return new CalculationPerformed(Id, c.Representation,
                        c.Parameters, result.Value, result.FunctionUsages.ToArray()).AsCommandResult();
                
                case AddFunction a:
                    if (_calculatorEngine.CanAddFunction(a.Definition.Expression, a.Definition.Parameters))
                        return new FunctionAdded(Id, a.Definition).AsCommandResult();
                    else throw new CannotAddFunctionException();
                
                default: throw new UnknownCommandException();
            }
        }
    }
}