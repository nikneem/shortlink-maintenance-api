using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Charts.Contracts;
using HexMaster.ShortLink.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HexMaster.ShortLink.Maintenance.Functions.Charts
{
    public  class ChartGetSparkLineFunction
    {
        private readonly IChartsService _service;
        private readonly ILogger<ChartGetSparkLineFunction> _logger;

        [FunctionName("ChartGetSparkLineFunction")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "charts/{shortCode}/sparkline")] HttpRequestMessage req,
            string shortCode)
        {
            try
            {
                var chartData = await _service.GetSparkChartsAsync(shortCode);
                return new OkObjectResult(chartData);
            }            catch (ShortLinkException shortLinkException)
            {
                _logger.LogWarning(shortLinkException, shortLinkException.Message);
                return new ConflictObjectResult(shortLinkException.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public ChartGetSparkLineFunction(
            IChartsService service, 
            ILogger<ChartGetSparkLineFunction> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
