using System.IO;
using Akka.Configuration;
using MightyCalc.Configuration;


namespace MightyCalc.LightHouse
{
    public class LighthouseConfig
    {
        public Akka.Configuration.Config Akka { get; }
        public string ClusterName { get; }

        public LighthouseConfig()
        {
            Akka = ConfigurationFactory.FromResource<LighthouseConfig>("MightyCalc.LightHouse.akka.conf")
                                       .InitFromEnvironment()
                                       .WithFallback(HoconConfigurations.FullDebug);
            ClusterName = Akka.GetString("lighthouse.actorsystem");
        }
    }
}