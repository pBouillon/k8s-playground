using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) { }
    }
}