using Microsoft.EntityFrameworkCore;
using MinimalApiInitial.Entities;

namespace MinimalApiInitial.Persistence;

public class CustomerDb : DbContext
{
    public CustomerDb(DbContextOptions<CustomerDb> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
}