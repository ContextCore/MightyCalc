namespace MightyCalc.Node
{
    public interface INamedCalculatorPool
    {
        IRemoteCalculator For(string name);
    }
}