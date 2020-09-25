using System;
using HexMaster.ShortLink.Resolver;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace HexMaster.ShortLink.Resolver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Configure your services here
            Console.WriteLine("Komt hier");
            
        }
    }
}


