using System;
using HexMaster.ShortLink.Core.Configuration;
using HexMaster.ShortLink.Resolver;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace HexMaster.ShortLink.Resolver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();


            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.ConfigureCore(configuration);
        }
    }
}


