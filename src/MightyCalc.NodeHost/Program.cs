using System;
using System.Threading.Tasks;

namespace MightyCalc.NodeHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var nodeService = new NodeService();
            nodeService.Start();
            Console.WriteLine("Press Control + C to terminate.");

            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                await nodeService.StopAsync();
            };

            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await nodeService.StopAsync();
            };
            
            await nodeService.TerminationHandle; 
            
            Console.WriteLine("Starting MightyCalc node");
        }
      
    }
}