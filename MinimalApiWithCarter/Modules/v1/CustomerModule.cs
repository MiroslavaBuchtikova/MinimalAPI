using AutoMapper;
using Carter;
using Microsoft.EntityFrameworkCore;
using MinimalApiFull.Dtos;
using MinimalApiFull.Entities;
using MinimalApiFull.Exceptions;
using MinimalApiFull.Filters;
using MinimalApiFull.Persistence;
using ILogger = Serilog.ILogger;

namespace MinimalApiFull.Modules.v1;

public class CustomerModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var appGroup = app.NewVersionedApi("Customers").HasApiVersion(1.0)
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory).RequireAuthorization();

        appGroup.MapPost("/customers", CreateCustomer);
        appGroup.MapPut("/customers/{id}", UpdateCustomer);
        appGroup.MapGet("/customers/{id}", GetCustomer);
        appGroup.MapGet("/customers", GetCustomers);
        appGroup.MapDelete("/customers/{id}", DeleteCustomer);
    }

    private async Task<IResult> CreateCustomer([Validate] CustomerDto customerDto, CustomerDb customerDb,
        IMapper mapper, ILogger logger)
    {
        if (customerDto.EmailAddress.ToUpper().Contains("GMAIL"))
        {
            throw new CustomException("Gmail is not allowed contact address");
        }

        var customer = mapper.Map<Customer>(customerDto);
        customerDb.Customers.Add(customer);
        await customerDb.SaveChangesAsync();

        logger.Information("CreateCustomerModule was created!");
        return Results.Created($"/customers/{customer.Id}", customer);
    }

    private async Task<IResult> UpdateCustomer(Guid id, [Validate] CustomerDto customerDto, CustomerDb customerDb,
        IMapper mapper)
    {
        var customer = await customerDb.Customers.FindAsync(id);

        if (customer is null) return Results.NotFound();

        mapper.Map(customerDto, customer);

        await customerDb.SaveChangesAsync();

        return Results.NoContent();
    }

    private async Task<IResult> DeleteCustomer(Guid id, CustomerDb customerDb, IMapper mapper, ILogger logger)
    {
        if (await customerDb.Customers.FindAsync(id) is Customer customer)
        {
            customerDb.Customers.Remove(customer);
            await customerDb.SaveChangesAsync();
            return Results.Ok(mapper.Map<CustomerDto>(customer));
        }

        return Results.NotFound();
    }

    private async Task<IResult> GetCustomer(Guid id, CustomerDb customerDb, IMapper mapper)
    {
        var result = await customerDb.Customers.FindAsync(id)
            is Customer customer
            ? Results.Ok(mapper.Map<CustomerDto>(customer))
            : Results.NotFound();
        return result;
    }

    private async Task<List<CustomerDto>> GetCustomers(CustomerDb customerDb, IMapper mapper)
    {
        return await customerDb.Customers.Select(x => mapper.Map<CustomerDto>(x)).ToListAsync();
    }
}