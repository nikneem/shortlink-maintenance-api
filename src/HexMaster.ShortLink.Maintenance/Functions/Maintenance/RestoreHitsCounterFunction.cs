using System;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Hits.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions.Maintenance
{
    public  class RestoreHitsCounterFunction
    {
        private readonly IHitsService _service;

        [FunctionName("RestoreHitsCounterFunction")]
        public  async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo tmr, ILogger log)
        {
            await _service.RestoreHitsCount();
        }

        public RestoreHitsCounterFunction(IHitsService service)
        {
            _service = service;
        }

    }
}
