using Akka.Configuration;

namespace MightyCalc.Node
{
    public static class HoconConfigurations
    {
        public static Config FullDebug { get; } = ConfigurationFactory.FromResource<AkkaCalculatorPool>("MightyCalc.Node.fulldebug.conf");
    }
}