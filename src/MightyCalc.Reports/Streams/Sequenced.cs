namespace MightyCalc.Reports.Streams
{
    public class Sequenced<T>
    {
        public T Message { get; set; }
        public long Sequence { get; set; }
    }
}