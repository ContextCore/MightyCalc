using Akka.Configuration;

namespace MightyCalc.Configuration
{
    public static class ConfigExtensions{
        public static Config InitFromEnvironment(this Config cfg)
        {
            return Configuration.GetEnvironmentConfig().WithFallback(cfg);

        }
    }
}