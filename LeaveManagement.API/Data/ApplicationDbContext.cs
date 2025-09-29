using Microsoft.EntityFrameworkCore;
using LeaveManagement.API.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LeaveManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var validFrom = new DateTime(2025, 1, 1);
            var validTo = new DateTime(2025, 12, 31);

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Employee" }
            );

            // Seed Leave Types
            modelBuilder.Entity<LeaveType>().HasData(
      new LeaveType { Id = 1, Type = "Casual Leave", Description = "Casual leave for personal work", ValidFrom = validFrom, ValidTo = validTo },
      new LeaveType { Id = 2, Type = "Sick Leave", Description = "Medical leave", ValidFrom = validFrom, ValidTo = validTo },
      new LeaveType { Id = 3, Type = "Leave Without Pay", Description = "Unpaid leave", ValidFrom = validFrom, ValidTo = validTo }
  );

            // Seed Admin User
            //modelBuilder.Entity<User>().HasData(
            //    new User
            //    {
            //        Id = 1,
            //        FirstName = "Admin",
            //        LastName = "User",
            //        EmailAddress = "admin@company.com",
            //        Department = "IT",
            //        Designation = "Administrator",
            //        ContactNo = "9999999999",
            //        Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
            //        RoleId = 1
            //    }
            //);

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.User)
                .WithMany(u => u.Leaves)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.LeaveType)
                .WithMany(lt => lt.Leaves)
                .HasForeignKey(l => l.LeaveTypeId);

            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.User)
                .WithMany(u => u.LeaveBalances)
                .HasForeignKey(lb => lb.UserId);

            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.LeaveType)
                .WithMany(lt => lt.LeaveBalances)
                .HasForeignKey(lb => lb.LeaveTypeId);
        }
    }
}