using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using C2B_POE1.Models;

namespace C2B_POE1.Data
{
    public class C2B_POE1Context : DbContext
    {
        public C2B_POE1Context (DbContextOptions<C2B_POE1Context> options)
            : base(options)
        {
        }

        public DbSet<C2B_POE1.Models.Product> Product { get; set; } = default!;
        public DbSet<C2B_POE1.Models.Category> Category { get; set; } = default!;
        public DbSet<C2B_POE1.Models.Order> Order { get; set; } = default!;
        public DbSet<C2B_POE1.Models.OrderLine> OrderLine { get; set; } = default!;
    }
}
