using System;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core;
using HexMaster.ShortLink.Core.Entities;
using HexMaster.ShortLink.Core.Hits.Contracts;
using HexMaster.ShortLink.Core.Models.Analytics;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions.Maintenance
{
    public  class ClickEventHandlerFunction
    {
        private readonly IHitsService _hitsService;

        [FunctionName("ClickEventHandlerFunction")]
        public  async Task Run(
            [EventHubTrigger(HubNames.ClickEventsHub, Connection = "CloudSettings:EventHubListenerConnectionString")] LinkClickedMessage events, 
            [Table(TableNames.Hits)] CloudTable table,
            ILogger log)
        {
            await _hitsService.RegisterHitAsync(events.Key, events.ClickedAt);
        }

        public ClickEventHandlerFunction(IHitsService hitsService)
        {
            _hitsService = hitsService;
        }
    }
}
