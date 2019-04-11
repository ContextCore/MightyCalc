using System;
using System.Collections.Generic;
using Akka.Configuration;
using MightyCalc.Configuration;
using MightyCalc.Node;

namespace MightyCalc.NodeHost
{
    public class NodeConfiguration
    {
	    public Akka.Configuration.Config AkkaConfig { get; set; }
        public string ReadModel { get; }
        
        public string ClusterName { get;}

        
        public NodeConfiguration()
        {
            var dbPort = 30020;
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel") ?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            ClusterName = Environment.GetEnvironmentVariable("MightyCalc_ClusterName") ?? "MightyCalc";
            
            var defaultConfig = ConfigurationFactory.FromResource<Program>("MightyCalc.NodeHost.akka.conf");
            AkkaConfig = defaultConfig.InitFromEnvironment().WithFallback(HoconConfigurations.FullDebug);
        }
    }
}