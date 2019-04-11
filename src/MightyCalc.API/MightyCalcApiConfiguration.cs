using System;
using System.Collections.Generic;
using Akka.Configuration;
using MightyCalc.Configuration;
using MightyCalc.Node;

namespace MightyCalc.API
{
    public class MightyCalcApiConfiguration
    {
        public Akka.Configuration.Config Akka { get; set; }
        public string ReadModel { get; set; }
        
        public string ClusterName { get; set; }

        public MightyCalcApiConfiguration()
        {
            var dbPort = 30020;
            
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel")?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            ClusterName = Environment.GetEnvironmentVariable("MightyCalc_ClusterName") ?? "MightyCalc";
            Akka = GetDefaultAkkaConfig().InitFromEnvironment();
        }
    
        private static Akka.Configuration.Config GetDefaultAkkaConfig()  
        {
            var cfg = ConfigurationFactory.FromResource<Startup>("MightyCalc.API.akka.conf");    
	        
            return cfg.WithFallback(HoconConfigurations.FullDebug);
        }
    }
} 