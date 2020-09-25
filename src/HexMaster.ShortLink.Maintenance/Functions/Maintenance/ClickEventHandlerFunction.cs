using System;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core;
using HexMaster.ShortLink.Core.Entities;
using HexMaster.ShortLink.Core.Models.Analytics;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions.Maintenance
{
    public static class ClickEventHandlerFunction
    {
        [FunctionName("ClickEventHandlerFunction")]
        public static async Task Run(
            [EventHubTrigger(HubNames.ClickEventsHub, Connection = "CloudSettings:EventHubListenerConnectionString")] LinkClickedMessage events, 
            [Table(TableNames.Hits)] CloudTable table,
            ILogger log)
        {
            var entity = new HitEntity
            {
                PartitionKey = PartitionKeys.Hit,
                RowKey = Guid.NewGuid().ToString(),
                CreatedOn = events.ClickedAt,
                ShortCode = events.Key,
                Timestamp = DateTimeOffset.UtcNow
            };

            var operation = TableOperation.Insert(entity);

            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(operation);
        }
    }
}
