using Akka.Cluster.Sharding;
using Akka.Cluster.Sharding.Serialization;
using Akka.Configuration;
using MightyCalc.IntegrationTests.Tools;
using MightyCalc.Node.Tests;
using Xunit;
using Xunit.Abstractions;
using Akka.Configuration;
using Akka.Persistence.Sql.Common.Journal;

namespace MightyCalc.Node.IntegrationTests
{
	public class PersistentNamedCalculatorTests:NamedCalculatorTests
    {
        public PersistentNamedCalculatorTests(ITestOutputHelper output):base(output,akkaConfig)
        {
	        var a = typeof(ClusterShardingMessageSerializer);
	        var b = typeof(Akka.Serialization.HyperionSerializer);
	        var c = typeof(IClusterShardingSerializable);
        }
    
          private static Config akkaConfig = @"
                                       akka.actor{
										    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster"" 
											serialize-messages = on 
										    serialize-creators = on
        							        serializers {
                                                hyperion = """+typeof(DebugHyperionSerializer).AssemblyQualifiedShortName()+@"""
                                            }
                                            serialization-bindings {
                                               ""System.Object"" = hyperion   
                                            }
                                       }    
                                      
	          
#akka.persistence.journal.plugin = ""akka.persistence.journal.postgresql""

akka.persistence{
	journal {
		plugin = ""akka.persistence.journal.postgresql""
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Journal.PostgreSqlJournal, Akka.Persistence.PostgreSql""

			# connection string used for database access
			connection-string = """+KnownConnectionStrings.Journal+@"""

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
		}
	}
}
";
       
    }
}