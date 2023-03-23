using AutoMapper;
using Carter;
using Microsoft.AspNetCore.Authorization;
using MinimalApiFull.Dtos;
using MinimalApiFull.Entities;
using MinimalApiFull.Exceptions;
using MinimalApiFull.Filters;
using MinimalApiFull.Persistence;
using MinimalApiFull.Services;

namespace MinimalApiFull.Modules.v2;

public class CustomerModuleV2 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var appGroup = app.NewVersionedApi("Customers").HasApiVersion(2.0).MapGroup("api/v2").AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory).RequireAuthorization();
        
        appGroup.MapGet("/customers", async (CustomerService customerService) =>
            {
                var customers = await customerService.GetCustomers();
                return Results.Ok(customers);
            })
            .WithOpenApi(operation => new(operation)
            {
                Deprecated = true
            });

        appGroup.MapGet("/customers/{id}", async (int id, CustomerDb db) =>
                await db.Customers.FindAsync(id)
                    is Customer customer
                    ? Results.Ok(customer)
                    : Results.NotFound())
            .ExcludeFromDescription();

        appGroup.MapPost("/customers", [Authorize]
            async ([Validate] CustomerDto customerDto, CustomerDb db, IMapper mapper) =>
            {
                var customer = mapper.Map<Customer>(customerDto);

                if (customer.EmailAddress.Contains("gmail"))
                {
                    throw new CustomException("Gmail is not allowed address");
                }

                db.Customers.Add(customer);
                await db.SaveChangesAsync();

                return Results.Created($"/customers/{customer.Id}", customer);
            });

        appGroup.MapPut("/customers/{id}",
            async (int id, [Validate] CustomerDto customerDto, CustomerDb db, IMapper mapper) =>
            {
                var customer = await db.Customers.FindAsync(id);

                if (customer is null) return Results.NotFound();

                mapper.Map(customerDto, customer);

                await db.SaveChangesAsync();

                return Results.NoContent();
            });

        appGroup.MapDelete("/customers/{id}", async (int id, CustomerDb db) =>
        {
            if (await db.Customers.FindAsync(id) is Customer customer)
            {
                db.Customers.Remove(customer);
                await db.SaveChangesAsync();
                return Results.Ok(customer);
            }

            return Results.NotFound();
        });
    }
}