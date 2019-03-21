using Akka.Persistence.Journal;

namespace MightyCalc.Node
{
    public class DomainEventAdapter : IEventAdapter
    {
        public string Manifest(object evt)
        {
            return string.Empty; // when no manifest needed, return ""
        }

        public object ToJournal(object evt)
        {
            return evt; // identity
        }

        public IEventSequence FromJournal(object evt, string manifest)
        {
            return EventSequence.Single(evt); // identity
        }
    }
}