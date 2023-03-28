using Microsoft.EntityFrameworkCore;
using MinimalApiNalejvarna.Entities;

namespace MinimalApiNalejvarna.Persistence;

public class CustomerDb : DbContext
{
    public CustomerDb(DbContextOptions<CustomerDb> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
}