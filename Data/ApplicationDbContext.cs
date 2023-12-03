using Document_Management.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Document_Management.Data
{
    public class ApplicationDbContext : DbContext
    {        
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
