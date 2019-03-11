using System.Collections.Generic;

namespace MightyCalc.Calculations
{
    public class Parameter
    {
        public Parameter(string name, double value=0)
        {
            Name = name;
            Value = value;
        }
        
        public string Name { get; }
        public double Value { get; }
    }
}