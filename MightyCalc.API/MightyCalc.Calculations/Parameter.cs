using System.Collections.Generic;

namespace MightyCalc.Calculations
{
    public class Parameter
    {
        public Parameter(string name, double value)
        {
            Name = name;
            Value = value;
        }
        
        public string Name { get; }
        public double Value { get; }
    }
}