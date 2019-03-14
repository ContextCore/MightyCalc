using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public class CalculatorActorProtocol
    {
        public class CalculateExpression
        {
            public string Representation { get; }
            public Parameter[] Parameters { get; }

            public CalculateExpression(string representation, Parameter[] parameters)
            {
                Representation = representation;
                Parameters = parameters;
            }
        }

        public class CalculationResult
        {
            public double Value { get; }
            
        }
    }
    
    
    public class CalculatorActor : ReceivePersistentActor
    {
        public CalculatorActor()
        {
            PersistenceId = Self.Path.Name;
            
        }
        
        public override string PersistenceId { get; }
    }
}