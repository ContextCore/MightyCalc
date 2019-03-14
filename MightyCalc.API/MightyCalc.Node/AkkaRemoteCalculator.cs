using System;
using System.Threading.Tasks;
using Akka.Actor;
using MightyCalc.Calculations;

namespace MightyCalc.Node
{
    public class AkkaRemoteCalculator : IRemoteCalculator
    {
        private readonly IActorRef _calculatorActorRegion;
        private readonly string _calculatorId;

        public AkkaRemoteCalculator(string calculatorId, IActorRef calculatorActorRegion)
        {
            _calculatorId = calculatorId;
            _calculatorActorRegion = calculatorActorRegion;
        }

        public async Task<double> Calculate(string expression, params Parameter[] parameters)
        {
            var res = await _calculatorActorRegion.Ask<CalculatorActorProtocol.CalculationResult>(new ShardEnvelop(
                _calculatorId,
                ShardIdGenerator.Instance.GetShardId(_calculatorId),
                new CalculatorActorProtocol.CalculateExpression(expression, parameters)),TimeSpan.FromSeconds(5));
            
            if (res is CalculatorActorProtocol.CalculationError e)
                throw e.Reason;
            
            return res.Value;
        }


        public async Task AddFunction(string functionName, string description, string expression, params string[] parameterNames)

        {
            var res = await _calculatorActorRegion.Ask<CalculatorActorProtocol.FunctionAdded>(new ShardEnvelop(
                    _calculatorId,
                    ShardIdGenerator.Instance.GetShardId(_calculatorId),
                    new CalculatorActorProtocol.AddFunction(new FunctionDefinition(functionName,parameterNames.Length,description,expression,parameterNames)))
                ,TimeSpan.FromSeconds(5));
            
            if (res is CalculatorActorProtocol.FunctionAddError e)
                throw e.Reason;
        }

        public async Task<FunctionDefinition[]> GetKnownFunction(string functionName)
        {
            var res = await _calculatorActorRegion.Ask<CalculatorActorProtocol.KnownFunctions>(new ShardEnvelop(
                    _calculatorId,
                    ShardIdGenerator.Instance.GetShardId(_calculatorId),
                    CalculatorActorProtocol.GetKnownFunctions.Instance)
                ,TimeSpan.FromSeconds(5));
            return res.Definitions;
        }
    }
}