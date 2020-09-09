using System;
using System.Net.Http;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core;
using HexMaster.ShortLink.Core.Entities;
using HexMaster.ShortLink.Core.Exceptions;
using HexMaster.ShortLink.Core.Helpers;
using HexMaster.ShortLink.Core.Models.ShortLinks;
using HexMaster.ShortLink.Core.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions
{
    public static class ShortLinkCreateFunction
    {

        [FunctionName("ShortLinkCreateFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            [Table(TableNames.ShortLinks)] CloudTable shortLinksTable,
            ILogger log)
        {
            try
            {
                var model = await req.Content.ReadAsAsync<ShortLinkCreateDto>();
                await ShortLinkCreateValidator.ValidateModelAsync(model);

                var newModel = await CreateNewShortLinkAsync(shortLinksTable, model);

                return new CreatedResult("https://app.4dn.me", newModel);
            }
            catch (ModelValidationException validationEx)
            {
                log.LogWarning(validationEx, "Validation error occurred while trying to add a new short link");
                return new BadRequestObjectResult(validationEx.Errors);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static async Task<ShortLinkDetailsDto> CreateNewShortLinkAsync(CloudTable shortLinksTable, ShortLinkCreateDto model)
        {
            var shortCode = await GenerateShortCode(shortLinksTable);
            var shortLinkEntity = new ShortLinkEntity
            {
                PartitionKey = PartitionKeys.ShortLinks,
                RowKey = Guid.NewGuid().ToString(),
                EndpointUrl = model.EndpointUrl,
                ShortCode = shortCode,
                CreatedOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMonths(3),
                OwnerId = "bananas",
                TotalHits = 0,
                Timestamp = DateTimeOffset.UtcNow
            };
            var tableOperation = TableOperation.Insert(shortLinkEntity);
            await shortLinksTable.ExecuteAsync(tableOperation);
            return ShortLinkDetailsDto.CreateFromEntity(shortLinkEntity);
        }

        private static async Task<string> GenerateShortCode(CloudTable table)
        {
            bool codeIsUnique;
            string shortCode;
            do
            {
                shortCode = ShortCodeGenerator.GenerateShortCode();
                var partitionKeyFilter = TableQuery.GenerateFilterCondition(
                    nameof(ShortLinkEntity.PartitionKey), 
                    QueryComparisons.Equal,
                    PartitionKeys.ShortLinks);
                var shortCodeFilter = TableQuery.GenerateFilterCondition(
                    nameof(ShortLinkEntity.ShortCode), 
                    QueryComparisons.Equal,
                    shortCode);

                var queryFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, shortCodeFilter);
                var query = new TableQuery<ShortLinkEntity>().Where(queryFilter);
                var segment = await table.ExecuteQuerySegmentedAsync(query, null);

                codeIsUnique = segment.Results.Count == 0;
            } while (!codeIsUnique);

            return shortCode;
        }

    }
}
