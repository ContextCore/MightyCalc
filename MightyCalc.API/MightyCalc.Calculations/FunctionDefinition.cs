namespace MightyCalc.Calculations
{
    public class FunctionDefinition
    {
        public FunctionDefinition(string name, int arity, string description, string expression, params string [] parameters)
        {
            Name = name;
            Arity = arity;
            Description = description;
            Expression = expression;
            Parameters = parameters;
        }

        public string Name { get; }
        public string Description { get; }
        public int Arity { get; }
        public string Expression { get; }
        public string[] Parameters { get; }
    }
}