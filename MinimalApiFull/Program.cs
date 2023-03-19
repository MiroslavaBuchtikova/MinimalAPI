using System.Text;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MinimalApiFull;
using MinimalApiFull.Middlewares;
using MinimalApiFull.Persistence;
using MinimalApiFull.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CustomerDb>(opt => opt.UseInMemoryDatabase("CustomerList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<CustomerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning()
    .AddApiExplorer()
    .EnableApiVersionBinding();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);
builder.Services.AddLogging(logging => { logging.AddConsole(); });

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCarter();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    // Log the request
    logger.LogInformation($"{context.Request.Method} {context.Request.Path}");

    // Read the request body
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();

    // Log the request body
    logger.LogInformation($"Request body: {requestBody}");

    // Replace the request body stream with a new MemoryStream
    var buffer = Encoding.UTF8.GetBytes(requestBody);
    context.Request.Body = new MemoryStream(buffer);

    // Call the next middleware in the pipeline
    await next();

    // Log the response
    logger.LogInformation($"Status code: {context.Response.StatusCode}");
});

app.UseMiddleware<CustomErrorHandlerMiddleware>();
app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            var descriptions = app.DescribeApiVersions();

            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
                options.RoutePrefix = String.Empty;
            }
        });
}

app.Run();