using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Models;

namespace OneSWebBooking.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Area> Areas { get; set; }
        public DbSet<ComputerCategory> ComputerCategories { get; set; }
        public DbSet<Computer> Computers { get; set; }
        //public DbSet<User> Users { get; set; }
    }
}