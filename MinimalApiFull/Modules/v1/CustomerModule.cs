using AutoMapper;
using Carter;
using MinimalApiFull.Dtos;
using MinimalApiFull.Entities;
using MinimalApiFull.Filters;
using MinimalApiFull.Persistence;
using MinimalApiFull.Services;

namespace MinimalApiFull.Modules.v1;

public class CustomerModuleV1 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var appGroup = app.NewVersionedApi("Customers").HasApiVersion(1.0)
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory).RequireAuthorization();

        appGroup.MapGet("/customers", async (CustomerService customerService) =>
            {
                var customers = await customerService.GetCustomers();
                return Results.Ok(customers);
            })
            .WithOpenApi(operation => new(operation)
            {
                Deprecated = true
            });

        appGroup.MapGet("/customers/{id}", async (Guid id, CustomerDb db) =>
                await db.Customers.FindAsync(id)
                    is Customer customer
                    ? Results.Ok(customer)
                    : Results.NotFound())
            .ExcludeFromDescription();

        appGroup.MapPut("/customers/{id}",
            async (int id, [Validate] CustomerDto customerDto, CustomerDb db, IMapper mapper) =>
            {
                var customer = await db.Customers.FindAsync(id);

                if (customer is null) return Results.NotFound();

                mapper.Map(customerDto, customer);

                await db.SaveChangesAsync();

                return Results.NoContent();
            });

        appGroup.MapDelete("/customers/{id}", async (Guid id, CustomerDb db) =>
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