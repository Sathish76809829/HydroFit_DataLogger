using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// For authorizing user access for ASP.NET request
    /// </summary>
    public class AuthenticationService : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AuthenticationService(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase)
                    && password.Equals("Admin@4321"))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "admin"),
                        new Claim(ClaimTypes.Name, username),
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);

                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Error Occured.Authorization failed."));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid Credentials"));
        }
    }
}
