using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MightyCalc.Configuration;
using MightyCalc.Node;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.NodeHost
{
    public class NodeService
    {
        private ActorSystem _nodeSystem;

        public void Start()
        {
            var config = new NodeConfiguration();
            _nodeSystem = ActorSystem.Create(config.ClusterName, config.AkkaConfig);
            var cluster = Cluster.Get(_nodeSystem);
            cluster.RegisterOnMemberUp(() => OnStart(_nodeSystem, config.ReadModel));
            
        }

        /// <summary>
        /// Task completes once the Lighthouse <see cref="ActorSystem"/> has terminated.
        /// </summary>
        /// <remarks>
        /// Doesn't actually invoke termination. Need to call <see cref="StopAsync"/> for that.
        /// </remarks>
        public Task TerminationHandle => _nodeSystem.WhenTerminated;

        private static void OnStart(ActorSystem system, string readModelConnectionString)
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseNpgsql(readModelConnectionString)
                .Options;


            using (var myDbContext = new FunctionUsageContext(options))
            {
                myDbContext.Database.Migrate();
            }

            system.InitReportingExtension(new ReportingDependencies(options)).Start();


            var pool = new AkkaCalculatorPool(system);
        }

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(_nodeSystem).Run(new ManualRequestReason());
        }
        
        private static string ExecutingAssemblyFolder()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var executingAssemblyFolder = Path.GetDirectoryName(location);
            return executingAssemblyFolder;
        }
    }
}