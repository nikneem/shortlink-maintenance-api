using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace HexMaster.ShortLink.Resolver.Functions
{
    public static class RedirectToAppFunction
    {
        [FunctionName("RedirectToAppFunction")]
        public static IActionResult RedirectToApp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route =  "/")]
            HttpRequest req)
        {
            return new RedirectResult("https://app.4dn.me/", true);
        }
    }
}
