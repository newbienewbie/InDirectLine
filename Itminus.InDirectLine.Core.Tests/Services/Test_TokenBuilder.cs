using Itminus.InDirectLine.Core.Authentication;
using Itminus.InDirectLine.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.InDirectLine.Core.Tests.Services
{
    public class TokenBuilderFixture : IDisposable
    {
        public TokenBuilderFixture()
        {
            var services= new ServiceCollection();
            services.AddOptions<InDirectLineAuthenticationOptions>().Configure(opts=> {
                opts.Audience = "this.is.audience";
                opts.Issuer = "this.is.issuer";
                opts.Key = "this.is.a/T0p/s3cr3t!k3y*.for-unit-test";
            });

            services.AddOptions<TokenValidationParameters>().Configure<IOptions<InDirectLineAuthenticationOptions>>((opts, dlOpts)=> {
                opts.ValidateIssuer = true;
                opts.ValidateAudience = true;
                opts.ValidateLifetime = true;
                opts.ValidateIssuerSigningKey = true;
                opts.ValidIssuer = dlOpts.Value.Issuer;
                opts.ValidAudience = dlOpts.Value.Audience;
                opts.IssuerSigningKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(dlOpts.Value.Key));
            });

            services.AddSingleton<TokenBuilder>();
            this.ServiceProvider = services.BuildServiceProvider();
            this.TokenBuilder = ServiceProvider.GetRequiredService<TokenBuilder>();
            this.InDirectionAuthenticationOptions = ServiceProvider.GetRequiredService<IOptions<InDirectLineAuthenticationOptions>>().Value;
        }

        public TokenBuilder TokenBuilder { get;}
        public InDirectLineAuthenticationOptions InDirectionAuthenticationOptions { get; }
        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
        }
    }

    public class Test_TokenBuilder : IClassFixture<TokenBuilderFixture>
    {
        private readonly TokenBuilder _tokenBuilder;
        private readonly TokenBuilderFixture _fixture;

        public Test_TokenBuilder(TokenBuilderFixture fixture)
        {
            this._fixture = fixture;
            this._tokenBuilder = fixture.TokenBuilder;
        }

        [Theory]
        [InlineData("itminus", 2000, "abc","efg","opq")]
        [InlineData("username2", 3000, "rst","uvw","xyz")]
        public Task Test_CreateToken(string username, int expires, params string[] claimstrs ) 
        {
            var claims = claimstrs.Select(str => new Claim("MyClaimType", str)).ToList();
            var token = this._tokenBuilder.BuildToken(username,claims,expires);


            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = this._fixture.ServiceProvider.GetRequiredService<IOptions<TokenValidationParameters>>().Value;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            Assert.Equal( username, principal.Identity.Name);
            Assert.Equal(_fixture.InDirectionAuthenticationOptions.Issuer, validatedToken.Issuer);
            foreach (var claim in claims)
            { 
                Assert.True(principal.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value));
            }
            return Task.CompletedTask;
        }

    }
}
