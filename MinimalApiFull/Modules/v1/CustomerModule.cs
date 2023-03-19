using AutoMapper;
using Carter;
using MinimalApiFull.Dtos;
using MinimalApiFull.Entities;
using MinimalApiFull.Exceptions;
using MinimalApiFull.Filters;
using MinimalApiFull.Persistence;
using MinimalApiFull.Services;

namespace MinimalApiFull.Modules.v1;

public class CustomerModuleV1 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var customersVersionApi = app.NewVersionedApi("Customers");
        var customersV1 = customersVersionApi.MapGroup("/api/v1").HasApiVersion(1.0)
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);
        
        customersV1.MapGet("/customers", async (CustomerService customerService) =>
            {
                var customers = await customerService.GetCustomers();
                return Results.Ok(customers);
            })
            .WithOpenApi(operation => new(operation)
            {
                Deprecated = true
            });

        customersV1.MapGet("/customers/{id}", async (int id, CustomerDb db) =>
                await db.Customers.FindAsync(id)
                    is Customer customer
                    ? Results.Ok(customer)
                    : Results.NotFound())
            .ExcludeFromDescription();

        customersV1.MapPost("/customers",
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

        customersV1.MapPut("/customers/{id}",
            async (int id, [Validate] CustomerDto customerDto, CustomerDb db, IMapper mapper) =>
            {
                var customer = await db.Customers.FindAsync(id);

                if (customer is null) return Results.NotFound();

                mapper.Map(customerDto, customer);

                await db.SaveChangesAsync();

                return Results.NoContent();
            });

        customersV1.MapDelete("/customers/{id}", async (int id, CustomerDb db) =>
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