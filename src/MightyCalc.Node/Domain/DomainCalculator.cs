using System;
using System.Threading.Tasks;
using MightyCalc.Calculations;
using MightyCalc.Calculations.Aggregate.Commands;

namespace MightyCalc.Node.Domain
{
    public class DomainCalculator : IRemoteCalculator
    {
        private readonly CalculatorCommandsHandler _commandHandler;
        private readonly string _calculatorId;

        public DomainCalculator(string calculatorId, CalculatorCommandsHandler commandHandler)
        {
            _calculatorId = calculatorId;
            _commandHandler = commandHandler;
        }
        public async Task<double> Calculate(string expression, params Parameter[] parameters)
        {
            return await _commandHandler.Execute(new CalculateExpression(_calculatorId, expression, parameters));
        }

        public async Task AddFunction(string functionName, string description, string expression, params string[] parameterNames)
        {
            await _commandHandler.Execute(new AddFunction(_calculatorId, new FunctionDefinition(functionName,parameterNames.Length,description,expression,parameterNames)));
        }
    }
}