using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MightyCalc.Calculations;

namespace MightyCalc.Node
{
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
}