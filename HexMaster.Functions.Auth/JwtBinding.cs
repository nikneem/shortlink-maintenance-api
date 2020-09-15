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

        private AuthModel BuildItemFromAttribute(JwtBindingAttribute arg)
        {
            if (_http.HttpContext != null)
            {
                return new AuthModel
                {
                    IsAuthenticated = _http.HttpContext.User.Identity.IsAuthenticated
                };
            }
            return new AuthModel
            {
                IsAuthenticated = false
            };
        }
    }
}
