using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MinimalApiFull.Dtos;
using MinimalApiFull.Entities;
using MinimalApiFull.Exceptions;
using MinimalApiFull.Persistence;
using ILogger = Serilog.ILogger;

namespace MinimalApiFull.Services;

public class CustomerService
{
    private readonly CustomerDb _customerDb;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public CustomerService(CustomerDb customerDb, IMapper mapper, ILogger logger)
    {
        _customerDb = customerDb;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<List<Customer>> GetCustomers()
    {
        return await _customerDb.Customers.ToListAsync();
    }
    public async Task<IResult> SaveCustomer(CustomerDto customerDto)
    {
        if (customerDto.EmailAddress.Contains("gmail"))
        {
            throw new CustomException("Gmail is not allowed contact address");
        }

        var customer = _mapper.Map<Customer>(customerDto);
        _customerDb.Customers.Add(customer);
        await _customerDb.SaveChangesAsync();

        _logger.Information("Customer was created!");
        return Results.Created($"/customers/{customer.Id}", customer);
    }
}