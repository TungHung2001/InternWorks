using DataContext_test.Entities;
using DatacontextTest2.Entities;
using IdentityTest2.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatacontextTest2
{
    public class SchoolContext : IdentityDbContext<AppUser>
    {
        /* public SchoolContext()
        : base("DefaultConnection") // <-- this is what i added.
         {
         }*/
        public SchoolContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=TUNGHUNG;Database=SchoolDB1;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedsRole(builder);
        }

        private static void SeedsRole(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
                (
                    new IdentityRole() { Name = "Admin", ConcurrencyStamp ="1", NormalizedName="Admin"},
                     new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
                      new IdentityRole() { Name = "Hr", ConcurrencyStamp = "3", NormalizedName = "Hr" }

                );
        }
    }
}
