using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MightyCalc.Reports.DatabaseProjections;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;

namespace MightyCalc.API
{
    public class Program
    {
        public static int Main(string[] args)
        {
            
            try
            {
                var host = CreateHostBuilder(args).Build();
                
                using(var scope = host.Services.CreateScope())
                {
                    var myDbContext = scope.ServiceProvider.GetRequiredService<FunctionUsageContext>();
                    Console.WriteLine("Migrating db at " + myDbContext.Database.GetDbConnection().ConnectionString);
                    myDbContext.Database.Migrate();
                }

            
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseSerilog();
                });
    }
}