using Akka.Actor;

namespace MightyCalc.Reports
{
    public class Projection
    {
        public string Name { get; set; }
        public string Projector { get; set; }
        public string Event { get; set; }
        public long Sequence { get; set; }
    }
}