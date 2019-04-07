using System;
using System.Linq;
using Akka.Configuration;
using MightyCalc.Node;

namespace MightyCalc.API
{
    public class MightyCalcApiConfiguration
    {
        public Config Akka { get; set; }
        public string ReadModel { get; set; }
        
        public string[] SeedNodes { get; set; }

        public string ClusterName { get; set; }

        public MightyCalcApiConfiguration()
        {
            var dbPort = 30020;
            
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel")?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            ClusterName = Environment.GetEnvironmentVariable("MightyCalc_ClusterName") ?? "MightyCalc";
            SeedNodes = Environment.GetEnvironmentVariable("MightyCalc_SeedNodes")?.Split(";");
            var defaultConfig = GetDefaultAkkaConfig();
            Akka = defaultConfig;
            if (SeedNodes == null) return;
            
            Config seedNodesCfg =
                $"akka.cluster.seed-nodes = [{string.Join(",", SeedNodes.Select(s => $"\"{s}\""))}";
            
            Akka = seedNodesCfg.WithFallback(defaultConfig);
        }
    
        private static Config GetDefaultAkkaConfig()
        {
            var cfg = ConfigurationFactory.FromResource<Startup>("MightyCalc.API.akka.conf");    
	        
            return cfg.WithFallback(HoconConfigurations.FullDebug);
        }
    }
}