using System;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;
using MightyCalc.Calculations;

namespace MightyCalc.Node
{
    public class CalculatorActor : ReceivePersistentActor
    {
        public CalculatorActor()
        {
            PersistenceId = Self.Path.Name;
            var calculator = new SpracheCalculator();
            
            Command<CalculatorActorProtocol.AddFunction>(a =>
            {
                try
                {
                    AddFunction(calculator, a.Definition);
                    Persist(new FunctionAdded(PersistenceId, a.Definition), e =>
                    {
                        Sender.Tell(CalculatorActorProtocol.FunctionAdded.Instance);
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new CalculatorActorProtocol.FunctionAddError(ex));
                    throw new FunctionAddException(ex);
                }
            });
            
            Command<CalculatorActorProtocol.GetKnownFunctions>(g =>
                Sender.Tell(new CalculatorActorProtocol.KnownFunctions(calculator.GetKnownFunctions().ToArray())));
            
            Command<CalculatorActorProtocol.CalculateExpression>(c =>
            {
                try
                {
                    var result = calculator.Calculate(c.Representation, c.Parameters);
                    PersistAsync(new CalculationPerformed(PersistenceId, c.Representation,
                        c.Parameters, result.FunctionUsages.ToArray()), e => { });
                    Sender.Tell(new CalculatorActorProtocol.CalculationResult(result.Value));
                }
                catch (Exception ex)
                {
                    Sender.Tell(new CalculatorActorProtocol.CalculationError(ex));
                }
            });
            Recover<FunctionAdded>(a => AddFunction(calculator, a.Definition));
            Recover<CalculationPerformed>(p => { });
        }

        public class FunctionAddException : Exception
        {
            public FunctionAddException(Exception exception) : base("failed to add new function", exception)
            {
            }
        }

        private static void AddFunction(SpracheCalculator calculator, FunctionDefinition functionDefinition)
        {
            calculator.AddFunction(functionDefinition.Name,
                functionDefinition.Description,
                functionDefinition.Expression,
                functionDefinition.Parameters);
        }

        public override string PersistenceId { get; }

        
        
        public class CalculationPerformed
        {
            public string CalculatorId { get; }
            public string Expression { get; }
            public Parameter[] Parameters { get; }
            public string[] FunctionsUsed { get; }

            public CalculationPerformed(string calculatorId, string expression, Parameter[] parameters,
                string[] functionsUsed)
            {
                CalculatorId = calculatorId;
                Expression = expression;
                Parameters = parameters;
                FunctionsUsed = functionsUsed;
            }
        }

        public class FunctionAdded
        {
            public string CalculatorId { get; }
            public FunctionDefinition Definition { get; }

            public FunctionAdded(string calculatorId, FunctionDefinition definition)
            {
                CalculatorId = calculatorId;
                Definition = definition;
            }
        }
    }
}