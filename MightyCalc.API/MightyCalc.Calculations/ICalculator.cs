namespace MightyCalc.Calculations
{
    public interface ICalculator
    {
        double Calculate(string expression, params Parameter[] parameters);
        void AddFunction(string name, string expression, params Parameter[] parameters);
    }
}