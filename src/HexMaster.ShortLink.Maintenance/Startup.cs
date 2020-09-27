using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HexMaster.ShortLink.Core.Configuration;
using HexMaster.ShortLink.Maintenance;

[assembly: FunctionsStartup(typeof(DiStartup))]
namespace HexMaster.ShortLink.Maintenance
{
    public class DiStartup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            
            builder.Services.ConfigureCore(configuration);
            builder.Services.AddApplicationInsightsTelemetry();

        }
    }
}


