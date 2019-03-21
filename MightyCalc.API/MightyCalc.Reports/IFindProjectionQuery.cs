namespace MightyCalc.Reports
{
    public interface IFindProjectionQuery
    {
        Projection Execute(string name, string projector, string eventName);
    }
}