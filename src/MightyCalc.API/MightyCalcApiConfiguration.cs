using System;
using Akka.Configuration;
using MightyCalc.Node;

namespace MightyCalc.API
{
    public class MightyCalcApiConfiguration
    {
        public Config AkkaConfig { get; set; }
        public string Akka { get; set; }
        public string ReadModel { get; set; }
        public string Snapshot { get; set; }
        public string Journal { get; set; }

        public MightyCalcApiConfiguration()
        {
            var dbPort = 30020;
            ReadModel = Environment.GetEnvironmentVariable("MightyCalc_ReadModel")  ?? $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
            Journal =  Environment.GetEnvironmentVariable("MightyCalc_Journal") ??  $"Host=localhost;Port={dbPort};Database=journal;User ID=postgres;";
            Snapshot =  Environment.GetEnvironmentVariable("MightyCalc_SnapshotStore") ?? $"Host=localhost;Port={dbPort};Database=snapshotstore;User ID=postgres;";
            AkkaConfig = GetDefaultAkkaConfig(Journal, Snapshot);
            Akka = AkkaConfig.ToString();
        }
    
        private static Config GetDefaultAkkaConfig(string journalConnectionString, string snapshotConnectionString)
        {
	        
		      Config clusterAndPersistenceConfig = @"
akka.actor.provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
akka.actor{
    #serializers {
	#     hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
    #}
    #serialization-bindings {
    #	  ""Akka.Cluster.Sharding.IClusterShardingSerializable, Akka.Cluster.Sharding"" = akka-sharding
    #     ""System.Object"" = hyperion
    #}
}
                        

akka.persistence.journal.plugin = ""akka.persistence.journal.postgresql""
akka.persistence.snapshot-store.plugin = ""akka.persistence.snapshot-store.postgresql""

akka.persistence.query.journal.sql {
			class = ""Akka.Persistence.Query.Sql.SqlReadJournalProvider, Akka.Persistence.Query.Sql""
		    refresh-interval = 1s
	        max-buffer-size = 1
    }

akka.persistence{
	journal {
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Journal.PostgreSqlJournal, Akka.Persistence.PostgreSql""

		    event-adapters {
                 tagging = """ + typeof(DomainEventAdapter).AssemblyQualifiedName + @"""
              }

            event-adapter-bindings {
                """ + typeof(IDomainEvent).AssemblyQualifiedName + @""" = tagging
            }
			# connection string used for database access
			connection-string = """ + journalConnectionString + @"""

			
			# default SQL commands timeout
			connection-timeout = 30s

			# PostgreSql schema name to table corresponding with persistent journal
			schema-name = public

			# PostgreSql table corresponding with persistent journal
			table-name = event_journal

			# should corresponding journal table be initialized automatically
			auto-initialize = on
			
			# metadata table
			metadata-table-name = metadata

			# Postgres data type for payload column. Allowed options: bytea, json, jsonb
			stored-as = bytea

			# Setting used to toggle sequential read access when loading large objects
			# from journals and snapshot stores.
			sequential-access = off
		}
	}
	snapshot-store {
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Snapshot.PostgreSqlSnapshotStore, Akka.Persistence.PostgreSql""

			# connection string used for database access
			connection-string = """ + snapshotConnectionString + @"""

			# PostgreSql schema name to table corresponding with persistent journal
			schema-name = public

			# PostgreSql table corresponding with persistent journal
			table-name = snapshot_store

			# should corresponding journal table be initialized automatically
			auto-initialize = on
		}
	}
}
";
	        
            var fullDebugConfig = ConfigurationFactory.ParseString(@"
akka.log-dead-letters-during-shutdown = true
akka.actor.debug.receive = true
akka.actor.debug.autoreceive = true
akka.actor.debug.lifecycle = true
akka.actor.debug.event-stream = true
akka.actor.debug.unhandled = true
akka.actor.debug.fsm = true
akka.actor.debug.router-misconfiguration = true
akka.log-dead-letters = true
akka.loglevel = DEBUG
akka.stdout-loglevel = DEBUG");

            return clusterAndPersistenceConfig.WithFallback(fullDebugConfig);
        }
    }
}