using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Akka.Configuration;
using MightyCalc.Configuration;
using MightyCalc.Node;

namespace MightCalc.NodeHost
{
    public class NodeConfiguration
    {
	    public Config AkkaConfig { get; set; }
        public string ReadModel { get; set; }
        
        public string ClusterName { get; set; }


      
        
        public NodeConfiguration()
        {
            var dbPort = 30020;
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel") ?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            ClusterName = Environment.GetEnvironmentVariable("MightyCalc_ClusterName") ?? "MightyCalc";
            
            var defaultConfig = ConfigurationFactory.FromResource<Program>("MightyCalc.NodeHost.akka.conf");

            var customCfg = Configuration.GetEnvironmentConfig(new Dictionary<string, string>
            {
	            {"MightyCalc_Journal", "akka.persistence.journal.postgresql.connection-string"},
	            {"MightyCalc_SnapshotStore", "akka.persistence.snapshot-store.postgresql.connection-string"},
	            {"MightyCalc_SeedNodes", "akka.cluster.seed-nodes"},
	            {"MightyCalc_NodePort", "akka.remote.dot-netty.tcp.port"},
	            {"MightyCalc_PublicHostName", "akka.remote.dot-netty.tcp.public-hostname"},
	            {"MightyCalc_PublicIP", "akka.remote.dot-netty.tcp.public-ip"}
            });
	       
            AkkaConfig = customCfg.WithFallback(defaultConfig).WithFallback(HoconConfigurations.FullDebug);
        }
    }
}