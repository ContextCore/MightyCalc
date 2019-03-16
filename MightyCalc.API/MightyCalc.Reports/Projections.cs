using Akka.Actor;

namespace MightyCalc.Reports
{
    public class Projections
    {
        public string Name { get; set; }
        public string Event { get; set; }
        public int Sequence { get; set; }
    }


    public class StatisticsProjector
    {
        public StatisticsProjector(ActorSystem system)
        {
            
        }
    }
}