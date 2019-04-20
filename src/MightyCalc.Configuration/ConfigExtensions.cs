using Akka.Actor;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Cluster.Sharding;
using Petabridge.Cmd.Host;

namespace MightyCalc.Configuration
{
    public static class ConfigExtensions{
        public static Config InitFromEnvironment(this Config cfg)
        {
            return Configuration.GetEnvironmentConfig().WithFallback(cfg);
        }
    }
}