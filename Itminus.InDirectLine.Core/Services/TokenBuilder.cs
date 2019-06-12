


using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Itminus.InDirectLine.Core.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Itminus.InDirectLine.Core.Services
{
    public class TokenBuilder
    {

        public const string ClaimTypeConversationID = "Claim_Type:Itminus.InDirectLine.Core:ConversationID";
        private InDirectLineAuthenticationOptions _authenticationOpts;

        public TokenBuilder(IOptions<InDirectLineAuthenticationOptions> inDirectLineAuthenticatoinOpts)
        {
            this._authenticationOpts = inDirectLineAuthenticatoinOpts.Value??throw new ArgumentNullException(nameof(inDirectLineAuthenticatoinOpts));
        }

        public string BuildToken(string userName, IList<Claim> claims, int expireTime)
        {
            if(string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if(claims==null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            claims.Add(new Claim(ClaimTypes.Name,userName));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._authenticationOpts.Key));
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiredAt = DateTime.UtcNow.AddSeconds(expireTime);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiredAt,
                Issuer = this._authenticationOpts.Issuer,
                Audience = this._authenticationOpts.Audience,
                SigningCredentials = sign,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string RefreshToken()
        {
            // todo
            return "";
        }


    }
}