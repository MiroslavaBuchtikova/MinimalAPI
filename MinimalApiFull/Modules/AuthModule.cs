using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace MinimalApiFull.Modules;

public class AuthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/security/getToken", [AllowAnonymous](IConfiguration configuration) =>
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

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return Results.Ok(jwtToken);
        });
    }
}