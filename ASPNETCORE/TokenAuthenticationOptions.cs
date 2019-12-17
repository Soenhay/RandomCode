using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Athentication
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "token auth";
        public string Scheme => DefaultScheme;
        public StringValues AuthKey { get; set; }
    }
}
