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
    public static class AggregateDailyHitsFunction
    {
        [FunctionName("AggregateDailyHitsFunction")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, 
            [Table(TableNames.Hits)] CloudTable table,
            [Table(TableNames.HitsPerDay)] CloudTable hourlyHitsTable,
            ILogger log)
        {
            log.LogInformation($"Consolidate all hits into daily aggregates");
            var startDate = DateTimeOffset.UtcNow.AddDays(-7);
            var endDate = DateTimeOffset.UtcNow;

            var pkCondition = TableQuery.GenerateFilterCondition(nameof(HitEntity.PartitionKey), QueryComparisons.Equal,
                PartitionKeys.Hit);
            var minDateFilter = TableQuery.GenerateFilterConditionForDate(nameof(HitEntity.CreatedOn),
                QueryComparisons.GreaterThanOrEqual, startDate);
            var maxDateFilter = TableQuery.GenerateFilterConditionForDate(nameof(HitEntity.CreatedOn),
                QueryComparisons.LessThanOrEqual, endDate);

            var queryFilter = TableQuery.CombineFilters(pkCondition, TableOperators.And,
                TableQuery.CombineFilters(minDateFilter, TableOperators.And, maxDateFilter));

            var ct = new TableContinuationToken();
            var query = new TableQuery<HitEntity>().Where(queryFilter);

            var aggregates = new List<HitsAggregate>();

            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, ct);
                ct = segment.ContinuationToken;

                foreach (var hit in segment.Results)
                {
                    var dateString = hit.CreatedOn.ToString("yyyyMMdd");
                    var aggregate = aggregates.FirstOrDefault(agg =>
                                        agg.ShortCode == hit.ShortCode && agg.DateString == dateString)
                                    ?? new HitsAggregate
                                    {
                                        ShortCode = hit.ShortCode,
                                        TotalHits = 0,
                                        DateString = dateString,
                                        RangeStart = hit.CreatedOn
                                            .AddHours(-hit.CreatedOn.Hour)
                                            .AddMinutes(-hit.CreatedOn.Minute)
                                            .AddSeconds(-hit.CreatedOn.Second)
                                            .AddMilliseconds(-hit.CreatedOn.Millisecond)
                                    };
                    aggregate.TotalHits++;
                    if (aggregate.TotalHits == 1)
                    {
                        aggregates.Add(aggregate);
                    }
                }

            } while (ct != null);


            var addAggragateOperation = new TableBatchOperation();
            foreach (var agg in aggregates)
            {
                var entity = new HitsAggregateDailyEntity
                {
                    PartitionKey = PartitionKeys.HitsPerDay,
                    RowKey = $"agg-daily-{agg.ShortCode}-{agg.DateString}",
                    TotalHits = agg.TotalHits,
                    Timestamp = DateTimeOffset.UtcNow,
                    DateString = agg.DateString,
                    AggregateRangeStart = agg.RangeStart,
                    AggregateRangeEnd = agg.RangeStart.AddDays(1),
                    ShortCode = agg.ShortCode
                };
                addAggragateOperation.Add(TableOperation.InsertOrReplace(entity));
                if (addAggragateOperation.Count == 100)
                {
                    await hourlyHitsTable.ExecuteBatchAsync(addAggragateOperation);
                    addAggragateOperation = new TableBatchOperation();
                }
            }

            if (addAggragateOperation.Count > 0)
            {
                await hourlyHitsTable.ExecuteBatchAsync(addAggragateOperation);
            }
        }
    }
}
