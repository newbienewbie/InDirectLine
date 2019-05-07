


using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Itminus.InDirectLine.Services
{
    public class TokenBuilder
    {
        private readonly IConfiguration _config;

        public TokenBuilder(IConfiguration config)
        {
            this._config = config;
        }

        public string BuildToken(string userName, IList<Claim> claims, int expireTime)
        {
            claims.Add(new Claim(ClaimTypes.Name,userName));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiredAt = DateTime.UtcNow.AddMinutes(expireTime);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiredAt,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
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