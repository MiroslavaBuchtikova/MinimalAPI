using Microsoft.EntityFrameworkCore;
using MinimalApiFull.Entities;
using MinimalApiFull.Persistence;

namespace MinimalApiFull.Services;

public class CustomerService
{
    private readonly CustomerDb _customerDb;

    public CustomerService(CustomerDb customerDb)
    {
        _customerDb = customerDb;
    }

    public async Task<List<Customer>> GetCustomers()
    {
        return await _customerDb.Customers.ToListAsync();
    }
}