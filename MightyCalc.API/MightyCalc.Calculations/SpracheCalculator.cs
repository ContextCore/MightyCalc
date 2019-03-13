using System;
using System.Collections.Generic;
using System.Linq;
using Sprache.Calc;

namespace MightyCalc.Calculations
{
    public class SpracheCalculator : ICalculator
    {
        readonly XtensibleCalculator _calculator = new XtensibleCalculator();
        private readonly List<FunctionDefinition> _knownFunctions = new List<FunctionDefinition>();
        public SpracheCalculator()
        { 
            AddFunction("add","Addition","a+b", "a", "b");
            AddFunction("sub","Substraction","a-b", "a", "b");
            AddFunction("mul","Multiply","a*b", "a", "b");
            AddFunction("div","Divide","a/b", "a", "b");
            
            AddFunction("sqrt","Square Root",d => Math.Sqrt(d));
            AddFunction("cuberoot","Cube Root",d => Math.Pow(d, 1 / 3.0));
            AddFunction("fact","Factorial",d => Enumerable.Range(1, (int) d).Aggregate(1, (elem, fact) => fact * elem));

        }

        public double Calculate(string expression, params Parameter[] parameters)
        {
            var dict = new Dictionary<string, double>();
            foreach (var parameter in parameters)
            {
                dict[parameter.Name] = parameter.Value;
            }

            return _calculator.ParseExpression(expression, dict).Compile().Invoke();
        }

        public void AddFunction(string name, string description, string expression, params string[] parameterNames)
        {
            _knownFunctions.Add(new FunctionDefinition(name,parameterNames.Count(),description, expression));
            _calculator.RegisterFunction(name, expression, parameterNames.ToArray());
        }
        private void AddFunction(string name, string description, Func<double,double> expression)
        {
            _knownFunctions.Add(new FunctionDefinition(name,1,description,""));
            _calculator.RegisterFunction(name, expression);
        }
        private void AddFunction(string name, string description, Func<double,double,double> expression)
        {
            _knownFunctions.Add(new FunctionDefinition(name,2,description,""));
            _calculator.RegisterFunction(name, expression);
        }


        public IReadOnlyList<FunctionDefinition> GetKnownFunctions()
        {
            return _knownFunctions;
        }
    }
}