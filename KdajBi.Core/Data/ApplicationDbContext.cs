﻿using System;
using System.Collections.Generic;
using System.Text;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KdajBi.Core
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        //public DbSet<UserCompany> UserCompanies { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Workplace> Workplaces { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ClientTag> ClientTags { get; set; }

        public DbSet<SmsCampaign> SmsCampaigns { get; set; }
        public DbSet<SmsMsg> SmsMsgs { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AppUser>().Ignore(e => e.FullName);
            //builder.Entity<AppUser>().Ignore(e => e.Companies);
            //builder.Entity<AppUser>(b =>
            //{
            //    //Each User can have many entries in the UserRole join table
            //    b.HasMany(e => e.UserRoles)
            //        .WithOne()
            //        .HasForeignKey(ur => ur.Id)
            //        .IsRequired();
            //});
            //builder.Entity<UserCompany>().HasIndex(p => new { p.UserId, p.CompanyId }).IsUnique(true);
            //builder.Entity<UserCompany>().Property(m => m.Permissions).IsRequired(false);
            builder.Entity<Company>(b =>
            {
                b.HasMany(e => e.CompanyLocation)
                    .WithOne()
                    .HasForeignKey(ur => ur.CompanyId)
                    .IsRequired();
            });

            builder.Entity<ClientTag>().HasIndex(p => new { p.ClientId, p.TagId }).IsUnique(true);
            builder.Entity<Tag>().HasIndex(p => new { p.Title }).IsUnique(true);

        }
    }
}
