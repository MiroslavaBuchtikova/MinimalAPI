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
                var issuer = configuration.GetSection("Jwt")["Issuer"];
                var audience = configuration.GetSection("Jwt")["Audience"];
                var secretKey = configuration.GetSection("Jwt")["Key"];

                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secretKey);

                var descriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = handler.CreateToken(descriptor);

                jwtToken = handler.WriteToken(token);
                cache.Set(cacheKey, jwtToken, TimeSpan.FromSeconds(25));
            }

            return Results.Ok(jwtToken);
        });
    }
}