using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core;
using HexMaster.ShortLink.Core.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions
{
    public static class AggregateHourlyHitsFunction
    {
        [FunctionName("AggregateHourlyHitsFunction")]
        public static async Task Run(
            [TimerTrigger("0 0 * * * *")]
            TimerInfo myTimer,
            [Table(TableNames.Hits)] CloudTable table,
            [Table(TableNames.HitsPerHour)] CloudTable hourlyHitsTable,
            ILogger log)
        {
            log.LogInformation($"Consolidate all hits into hourly aggregates");
            var startDate = DateTimeOffset.UtcNow.AddDays(-1);
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
                    var hourString = hit.CreatedOn.ToString("HH");
                    var aggregate = aggregates.FirstOrDefault(agg =>
                                        agg.ShortCode == hit.ShortCode && agg.DateString == dateString &&
                                        agg.HourString == hourString)
                                    ?? new HitsAggregate
                                    {
                                        ShortCode = hit.ShortCode,
                                        TotalHits = 0,
                                        DateString = dateString,
                                        HourString = hourString,
                                        RangeStart = hit.CreatedOn
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

                //// Split into chunks of 100 for batching
                //List<List<HitEntity>> rowsChunked = segment.Select((x, index) => new { Index = index, Value = x })
                //    .Where(x => x.Value != null)
                //    .GroupBy(x => x.Index / 100)
                //    .Select(x => x.Select(v => v.Value).ToList())
                //    .ToList();

                //foreach (List<HitEntity> rows in rowsChunked)
                //{
                //    cleanedEntries += rows.Count;
                //    TableBatchOperation tableBatchOperation = new TableBatchOperation();
                //    rows.ForEach(x => tableBatchOperation.Add(TableOperation.Delete(x)));
                //    await table.ExecuteBatchAsync(tableBatchOperation);
                //}

            } while (ct != null);


            var addAggragateOperation = new TableBatchOperation();
            foreach (var agg in aggregates)
            {
                var entity = new HitsAggregateHourlyEntity
                {
                    PartitionKey = PartitionKeys.HitsPerHour,
                    RowKey = $"agg-hourly-{agg.ShortCode}-{agg.DateString}-{agg.HourString}",
                    TotalHits = agg.TotalHits,
                    Timestamp = DateTimeOffset.UtcNow,
                    DateString = agg.DateString,
                    HourString = agg.HourString,
                    AggregateRangeStart = agg.RangeStart,
                    AggregateRangeEnd = agg.RangeStart.AddHours(1),
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

    public class HitsAggregate
    {
        public string ShortCode { get; set; }
        public int TotalHits { get; set; }
        public string DateString { get; set; }
        public string HourString { get; set; }
        public DateTimeOffset RangeStart { get; set; }
    }
}