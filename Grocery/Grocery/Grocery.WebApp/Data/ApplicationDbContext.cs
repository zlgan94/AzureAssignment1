using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using Grocery.WebApp.Models;
using Microsoft.AspNetCore.Identity;
using Grocery.WebApp.Data.Enums;

namespace Grocery.WebApp.Data
{
    public class ApplicationDbContext 
        : IdentityDbContext<MyIdentityUser, MyIdentityRole, Guid>
    {

        public DbSet<Product> Products { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // -- Any Model related customization is done using Fluent API,
            //    which cannot be done by DataAnnotations are handled here.

            builder.Entity<MyIdentityUser>()
                .Property(e => e.IsAdminUser)
                .HasDefaultValue(false);
            builder.Entity<MyIdentityUser>()
                .Property(e => e.Gender)
                .HasDefaultValue<MyAppGenderTypes>(MyAppGenderTypes.Male);

            builder.Entity<Product>()
                .Property(e => e.Quantity)
                .HasDefaultValue(0);
            builder.Entity<Product>()
                .Property(e => e.SellingPricePerUnit)
                .HasDefaultValue(0.00);
            builder.Entity<Product>()
                .Property(e => e.LastUpdatedOn)
                .HasDefaultValueSql("getdate()");

            // -- Define the Foreign Key policies
            //    for addressing CASCADE UPDATE and CASCADE DELETE
            //    NOTE: (p) Parent Entity (c) Child Entity
            builder.Entity<Product>()                           // child table
                .HasOne<MyIdentityUser>(c => c.CreatedByUser)   // object of parent in Child
                .WithMany(p => p.ProductsCreatedByUser)         // collection of children in Parent
                .HasForeignKey(c => c.CreatedByUserId)          // column of Child on which FK is established
                .OnDelete(DeleteBehavior.Restrict);             // CASCADE DELETE Behaviour
            builder.Entity<Product>()                           // child table
                 .HasOne<MyIdentityUser>(c => c.UpdatedByUser)   // object of parent in Child
                 .WithMany(p => p.ProductsUpdatedByUser)         // collection of children in Parent
                 .HasForeignKey(c => c.UpdatedByUserId)          // column of Child on which FK is established
                 .OnDelete(DeleteBehavior.Restrict);             // CASCADE DELETE Behaviour

            base.OnModelCreating(builder);
        }
    }
}
