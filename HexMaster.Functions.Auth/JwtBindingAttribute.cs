using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Description;

namespace HexMaster.Functions.Auth
{

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class JwtBindingAttribute : Attribute
    {
        public JwtBindingAttribute()
        {

        }

    }

}