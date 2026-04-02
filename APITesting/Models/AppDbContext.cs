using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace APITesting.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        //we're not using this for now 
        #region Required
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<User>()
        //        .Property(b => b.Email)
        //        .IsRequired();
        //}
        #endregion
    }
}
