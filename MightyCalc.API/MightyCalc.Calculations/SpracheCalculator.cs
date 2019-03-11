using System;
using System.Collections.Generic;
using System.Linq;
using Sprache.Calc;

namespace MightyCalc.Calculations
{
    public class SpracheCalculator : ICalculator
    {
        XtensibleCalculator _calculator = new XtensibleCalculator();

        public SpracheCalculator()
        {
            _calculator.RegisterFunction("fact",
                d => Enumerable.Range(1, (int) d).Aggregate(1, (elem, fact) => fact * elem));
            _calculator.RegisterFunction("sqrt",
                d => Math.Sqrt(d));
            _calculator.RegisterFunction("cuberoot",
                d => Math.Pow(d, 1 / 3.0));
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

        public void AddFunction(string name, string expression, params Parameter[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}