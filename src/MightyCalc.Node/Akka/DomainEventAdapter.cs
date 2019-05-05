using Akka.Persistence.Journal;

namespace MightyCalc.Node.Akka
{
    public class DomainEventAdapter : IEventAdapter
    {
        public string Manifest(object evt)
        {
            return string.Empty;
        }

        public object ToJournal(object evt)
        {
            return new Tagged(evt,new []{"aggregate",evt.GetType().Name}); 
        }

        public IEventSequence FromJournal(object evt, string manifest)
        {
            return EventSequence.Single(evt); 
        }
    }
}