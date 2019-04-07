using System;
using System.IO;
using System.Text;
using Akka.Configuration;
using MightyCalc.Node;

namespace MightCalc.NodeHost
{
    public class NodeConfiguration
    {
	    public Config AkkaConfig { get; set; }
        public string ReadModel { get; set; }
        public string Snapshot { get; set; }
        public string Journal { get; set; }

        public string ClusterName { get; set; }
        public NodeConfiguration()
        {
            var dbPort = 30020;
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel") ?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            Journal =  Environment.GetEnvironmentVariable("MightyCalc_Journal");
            Snapshot =  Environment.GetEnvironmentVariable("MightyCalc_SnapshotStore");
            ClusterName = Environment.GetEnvironmentVariable("MightyCalc_ClusterName") ?? "MightyCalc";
            var defaultConfig = ConfigurationFactory.FromResource<Program>("MightyCalc.NodeHost.akka.conf");
            
            var customCfg = new StringBuilder("");
            if (Journal != null)
            {
	            customCfg.AppendLine("akka.persistence.journal.postgresql.connection-string = " + Journal);
            }
            else
            {
	            Journal = defaultConfig.GetString("akka.persistence.journal.postgresql.connection-string");
            }
            
            if (Snapshot != null)
            {
	            customCfg.AppendLine("akka.persistence.snapshot-store.postgresql.connection-string = " + Journal);
            }
            else
            {
	            Snapshot = defaultConfig.GetString("akka.persistence.snapshot-store.postgresql.connection-string");
            }
            
            AkkaConfig = ((Config)customCfg.ToString()).WithFallback(defaultConfig).WithFallback(HoconConfigurations.FullDebug);
        }
    }
}