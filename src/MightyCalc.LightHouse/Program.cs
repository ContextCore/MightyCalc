using System;
using System.Linq;

namespace MightyCalc.LightHouse
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var lighthouseService = new LighthouseService();
            lighthouseService.Start();
            Console.WriteLine("Press Control + C to terminate.");

            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                await lighthouseService.StopAsync();
            };

            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await lighthouseService.StopAsync();
            };
            lighthouseService.TerminationHandle.Wait(); 
        }
    }
}