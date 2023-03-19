using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace MinimalApiFull.Modules;

public class AuthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/security/getToken", [AllowAnonymous](IConfiguration configuration, IMemoryCache cache) =>
        {
            var cacheKey = "auth_token";
 
            if (!cache.TryGetValue(cacheKey, out string jwtToken))
            {
                var issuer = configuration.GetSection("Jwt:Issuer").ToString();
                var audience = configuration.GetSection("Jwt:Audience").ToString();
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                var key = Encoding.ASCII.GetBytes(configuration.GetSection("Jwt:Key").ToString());


                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Expires = DateTime.UtcNow.AddHours(6),
                    Audience = audience,
                    Issuer = issuer,
                    SigningCredentials =
                        new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };

                var token = jwtTokenHandler.CreateToken(tokenDescriptor);

                jwtToken = jwtTokenHandler.WriteToken(token);

                cache.Set(cacheKey, jwtToken, TimeSpan.FromSeconds(10));
            }

            return Results.Ok(jwtToken);
        });
    }
}