using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.API
{
    public class MigratorStartupFilter: IStartupFilter
    {
        // We need to inject the IServiceProvider so we can create 
        // the scoped service, MyDbContext
        private readonly IServiceProvider _serviceProvider;
        public MigratorStartupFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var myDbContext = scope.ServiceProvider.GetRequiredService<FunctionUsageContext>();

                myDbContext.Database.Migrate();
            }

            return next;
        }
    }
}