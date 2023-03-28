using Microsoft.EntityFrameworkCore;
using MinimalApiFull.Entities;

namespace MinimalApiFull.Persistence;

public class CustomerDb : DbContext
{
    public CustomerDb(DbContextOptions<CustomerDb> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
}