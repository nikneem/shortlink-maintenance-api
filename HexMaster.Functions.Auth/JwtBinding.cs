using System.Net.Http;
using System.Net.Http.Headers;
using HexMaster.Functions.Auth.Helpers;
using HexMaster.Functions.Auth.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace HexMaster.Functions.Auth
{
    [Extension("JwtBinding")]
    public class JwtBinding : IExtensionConfigProvider
    {
        private readonly IHttpContextAccessor _http;

        public JwtBinding(IHttpContextAccessor http)
        {
            _http = http;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<JwtBindingAttribute>();
            rule.BindToInput(BuildItemFromAttribute);
        }

        private AuthorizedModel BuildItemFromAttribute(JwtBindingAttribute arg)
        {
            if (_http.HttpContext != null)
            {
                var authHeaderValue = _http.HttpContext.Request.Headers["Authorization"];
                AuthenticationHeaderValue headerValue = AuthenticationHeaderValue.Parse(authHeaderValue);
                var token =  TokenValidator.ValidateToken(headerValue,
                    arg.Audience,
                    arg.Issuer);
                return new AuthorizedModel
                {
                    IsAuthorized = _http.HttpContext.User.Identity.IsAuthenticated
                };
            }
            return new AuthorizedModel
            {
                IsAuthorized = false
            };
        }
    }
}
