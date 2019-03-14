using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence;
using MightyCalc.Calculations;

namespace MightyCalc.Node
{
    public interface INamedCalculatorPool
    {
        IRemoteCalculator For(string name);
    }

    public interface IRemoteCalculator
    {
        Task<double> Calculate(string expression, params Parameter[] parameters);

        Task AddFunction(string functionName, string description, string expression,
            params string[] parameterNames);
    }
    
    public static class CalculatorActorProtocol
    {
        public class CalculateExpression
        {
            public string Representation { get; }
            public Parameter[] Parameters { get; }

            public CalculateExpression(string representation, params Parameter[] parameters)
            {
                Representation = representation;
                Parameters = parameters;
            }
        }

        public class AddFunction
        {
            public FunctionDefinition Definition { get; }

            public AddFunction(FunctionDefinition definition)
            {
                Definition = definition;
            }
        }
        
        public class FunctionAdded
        {
            protected FunctionAdded()
            {
               
            }
            public static FunctionAdded Instance { get; } = new FunctionAdded();
        }

        public class FunctionAddError:FunctionAdded
        {
            public Exception Reason { get; }

            public FunctionAddError(Exception reason)
            {
                Reason = reason;
            }
        }


        public class CalculationResult
        {
            public CalculationResult(double value)
            {
                Value = value;
            }

            public double Value { get; }
        }

        public class CalculationError : CalculationResult
        {
            public Exception Reason { get; }

            public CalculationError(Exception reason) : base(0)
            {
                Reason = reason;
            }
        }
    }


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
                    Persist(new FunctionAdded(PersistenceId, a.Definition),e => {} );
                }
                catch (Exception ex)
                {
                    Sender.Tell(new CalculatorActorProtocol.FunctionAddError(ex));
                    throw new FunctionAddException(ex);
                }
                
            });
            Command<CalculatorActorProtocol.CalculateExpression>(c =>
            {
                try
                {
                    var result = calculator.Calculate(c.Representation, c.Parameters);
                    PersistAsync(new CalculationPerformed(PersistenceId, c.Representation,
                        c.Parameters, result.FunctionUsages.ToArray()), e =>{});
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
            public FunctionAddException(Exception exception):base("failed to add new function",exception)
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

        class CalculationPerformed
        {
            public string CalculatorId { get; }
            public string Expression { get; }
            public Parameter[] Parameters { get; }
            public string[] FunctionsUsed { get; }

            public CalculationPerformed(string calculatorId, string expression, Parameter[] parameters, string[] functionsUsed)
            {
                CalculatorId = calculatorId;
                Expression = expression;
                Parameters = parameters;
                FunctionsUsed = functionsUsed;
            }
        }

        class FunctionAdded
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