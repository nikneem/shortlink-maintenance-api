using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HexMaster.ShortLink.Core.Configuration;
using HexMaster.ShortLink.Maintenance;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: FunctionsStartup(typeof(DiStartup))]
namespace HexMaster.ShortLink.Maintenance
{
    public class DiStartup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            
            builder.Services.ConfigureCore(configuration);
            builder.Services.AddApplicationInsightsTelemetry();

        }
    }
}


