using Akka.Configuration;

namespace MightyCalc.Configuration
{
    public static class HoconConfigurations
    {
        public static Config FullDebug { get; } = ConfigurationFactory.FromResource<Cfg>("MightyCalc.Configuration.fulldebug.conf");
    }
}