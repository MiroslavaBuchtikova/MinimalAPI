using Carter;
using MinimalApiFull.Dtos;
using MinimalApiFull.Filters;
using MinimalApiFull.Services;

namespace MinimalApiFull.Modules.v1;

public class CreateCustomerModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var appV1 = app.NewVersionedApi("Customers").HasApiVersion(1.0)
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory).RequireAuthorization();

        appV1.MapPost("/customers",
            async ([Validate] CustomerDto customerDto, CustomerService customerService) =>
                await customerService.SaveCustomer(customerDto));

        var appV2 = app.NewVersionedApi("Customers").HasApiVersion(2.0)
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory).RequireAuthorization();

        appV2.MapPost("/customers",
            async ([Validate] CustomerDto customerDto, CustomerService customerService) =>
                await customerService.SaveCustomer(customerDto));

    }
}