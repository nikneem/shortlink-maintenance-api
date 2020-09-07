using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HexMaster.ShortLink.Data.Entities;
using HexMaster.ShortLink.Messages;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions
{
    public static class HitsCleanupFunction
    {
        [FunctionName("HitsCleanupFunction")]
        public static async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer,
            [Table(TableNames.Hits)] CloudTable table,
            ILogger log)
        {
            log.LogInformation($"Clean up all hits older than 7 days");
            var oldHits = DateTimeOffset.UtcNow.AddDays(-7);

            var pkCondition = TableQuery.GenerateFilterCondition(nameof(HitEntity.PartitionKey), QueryComparisons.Equal,
                PartitionKeys.Hit);
            var maxDateFilter = TableQuery.GenerateFilterConditionForDate(nameof(HitEntity.CreatedOn),
                QueryComparisons.LessThanOrEqual, oldHits);

            var queryFilter = TableQuery.CombineFilters(pkCondition, TableOperators.And, maxDateFilter);

            var ct = new TableContinuationToken();
            var query = new TableQuery<HitEntity>().Where(queryFilter);

            var cleanedEntries = 0;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, ct);
                ct = segment.ContinuationToken;

                // Split into chunks of 100 for batching
                List<List<HitEntity>> rowsChunked = segment.Select((x, index) => new {Index = index, Value = x})
                    .Where(x => x.Value != null)
                    .GroupBy(x => x.Index / 100)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                foreach (List<HitEntity> rows in rowsChunked)
                {
                    cleanedEntries += rows.Count;
                    TableBatchOperation tableBatchOperation = new TableBatchOperation();
                    rows.ForEach(x => tableBatchOperation.Add(TableOperation.Delete(x)));
                    await table.ExecuteBatchAsync(tableBatchOperation);
                }

            } while (ct != null);

            log.LogInformation($"Cleaned a total of {cleanedEntries} hits from the hits table");
        }
    }
}
