namespace MightyCalc.Reports.DatabaseProjections
{
    public class KnownFunction
    {
        public string CalculatorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Arity { get; set; }
        public string Expression { get; set; }
        public string Parameters { get; set; }
    }
}