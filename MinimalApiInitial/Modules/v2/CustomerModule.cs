using System.Net;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinimalApiInitial.Dtos;
using MinimalApiInitial.Entities;
using MinimalApiInitial.Persistence;

namespace MinimalApiInitial.Modules.v2;

public static class CustomerModule
{
    public static void RegisterCustomerModuleV2Endpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/customers", CreateCustomer).CreateCustomerEndpointFilter();
        endpoints.MapPut("/customers/{id}", UpdateCustomer);
        endpoints.MapGet("/customers/{id}", GetCustomer);
        endpoints.MapGet("/customers", GetCustomers);
        endpoints.MapDelete("/customers/{id}", DeleteCustomer);
    }

    private static async Task<IResult> CreateCustomer(CustomerDto customerDto, CustomerDb customerDb,
        IMapper mapper, IValidator<CustomerDto> validator, Serilog.ILogger logger)
    {
        var customer = mapper.Map<Customer>(customerDto);
        customerDb.Customers.Add(customer);
        await customerDb.SaveChangesAsync();

        logger.Information("CreateCustomerModule was created!");
        return Results.Created($"/customers/{customer.Id}", customer);
    }

    private static async Task<IResult> UpdateCustomer(Guid id, CustomerDto customerDto, CustomerDb customerDb,
        IMapper mapper)
    {
        var customer = await customerDb.Customers.FindAsync(id);

        if (customer is null) return Results.NotFound();

        mapper.Map(customerDto, customer);

        await customerDb.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCustomer(Guid id, CustomerDb customerDb, IMapper mapper)
    {
        if (await customerDb.Customers.FindAsync(id) is Customer customer)
        {
            customerDb.Customers.Remove(customer);
            await customerDb.SaveChangesAsync();
            return Results.Ok(mapper.Map<CustomerDto>(customer));
        }

        return Results.NotFound();
    }

    private static async Task<IResult> GetCustomer(Guid id, CustomerDb customerDb, IMapper mapper)
    {
        var result = await customerDb.Customers.FindAsync(id)
            is Customer customer
            ? Results.Ok(mapper.Map<CustomerDto>(customer))
            : Results.NotFound();
        return result;
    }

    private static async Task<List<CustomerDto>> GetCustomers(CustomerDb customerDb, IMapper mapper)
    {
        return await customerDb.Customers.Select(x => mapper.Map<CustomerDto>(x)).ToListAsync();
    }

    private static void CreateCustomerEndpointFilter(this RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.AddEndpointFilter(async (efiContext, next) =>
        {

            var customerDto = efiContext.GetArgument<CustomerDto>(0);
            var validator = efiContext.GetArgument<IValidator<CustomerDto>>(3);
            var logger = efiContext.GetArgument<Serilog.ILogger>(4);

            var validationResult = validator.Validate(customerDto);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary(),
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

            var result = await next(efiContext);

            logger.Information("CreateCustomer was created!");

            return result;
        });
    }



}