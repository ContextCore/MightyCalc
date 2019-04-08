using System;
using System.Collections.Generic;
using Akka.Configuration;
using MightyCalc.Node;

namespace MightyCalc.API
{
    public class MightyCalcApiConfiguration
    {
        public Config Akka { get; set; }
        public string ReadModel { get; set; }
        

        public string ClusterName { get; set; }

        public MightyCalcApiConfiguration()
        {
            var dbPort = 30020;
            
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel")?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            ClusterName = Environment.GetEnvironmentVariable("MightyCalc_ClusterName") ?? "MightyCalc";
            
            var customCfg = MightyCalc.Configuration.Configuration.GetEnvironmentConfig(new Dictionary<string, string>
            {
                {"MightyCalc_SeedNodes", "akka.cluster.seed-nodes"},
                {"MightyCalc_NodePort", "akka.remote.dot-netty.tcp.port"},
                {"MightyCalc_PublicHostName", "akka.remote.dot-netty.tcp.public-hostname"},
                {"MightyCalc_PublicIP", "akka.remote.dot-netty.tcp.public-ip"}
            });
            
            var defaultConfig = GetDefaultAkkaConfig();
            Akka = customCfg.WithFallback(defaultConfig);
        }
    
        private static Config GetDefaultAkkaConfig()
        {
            var cfg = ConfigurationFactory.FromResource<Startup>("MightyCalc.API.akka.conf");    
	        
            return cfg.WithFallback(HoconConfigurations.FullDebug);
        }
    }
}