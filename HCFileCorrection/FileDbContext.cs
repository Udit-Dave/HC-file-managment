using HCFileCorrection.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HCFileCorrection
{
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }
        public DbSet<DTRequestTable> HCPOSDownloadRequest { get; set; }
        public DbSet<DTFilesTable> HCPOSDownloadFiles { get; set; }
        //public DbSet<DTVendorPortalConfig> VendorPortalConfig { get; set; }
        public DbSet<DTDownloadQueue> HCPOSDownloadQueue { get; set; }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }

        public DbSet<DTCountry> HCPOSCountry { get; set; }

        public DbSet<DTUserSessionModel> UserSessions { get; set; }

        public DbSet<DTHCPOSVendorPortalConfig> HCPOSVendorPortalConfig { get; set; }
       
        public DbSet<DTRetailer> HCPOSRetailer { get; set; }
        public DbSet<DTUserMapping> HCPOSUserMapping { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DTFilesTable>()
                .HasOne(f => f.Request)
                .WithMany(r => r.Files)
                .HasForeignKey(f => f.RequestId);

            /*modelBuilder.Entity<DTHCPOSVendorPortalConfig>()
            .HasKey(c => c.Id);*/

            modelBuilder.Entity<DTCountry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.CountryDescription)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime2");
            });

            modelBuilder.Entity<DTRequestTable>()
           .HasOne(r => r.DownloadQueue)
           .WithMany(q => q.Requests) 
           .HasForeignKey(r => r.QueueID);

            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DTUserSessionModel>()
                 .HasKey(s => s.Session_Id);

            modelBuilder.Entity<DTUserSessionModel>()
                .HasOne(us => us.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<DTHCPOSVendorPortalConfig>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserName)
                    .IsUnicode();

                entity.Property(e => e.Password)
                    .IsUnicode();

                entity.Property(e => e.OtpString)
                    .IsUnicode();

                entity.Property(e => e.VendorPortalLink)
                    .IsUnicode();

                entity.Property(e => e.CreatedDateTime)
                    .HasColumnType("datetime2");

                entity.Property(e => e.UpdatedDateTime)
                    .HasColumnType("datetime2");

                entity.Property(e => e.PublisherTabs)
                    .HasDefaultValue(false);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.HCPOSVendorPortalConfigs)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Retailer)
                    .WithMany(p => p.HCPOSVendorPortalConfigs)
                    .HasForeignKey(d => d.RetailerId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<DTRetailer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RetailerName)
                    .IsRequired()
                    .IsUnicode();

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime2");
            });

            modelBuilder.Entity<DTUserMapping>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Country)
                    .WithMany(c => c.UserMappings)
                    .HasForeignKey(e => e.Country_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.Retailer)
                    .WithMany(r => r.UserMappings)
                    .HasForeignKey(e => e.Retailer_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserMappings)
                    .HasForeignKey(e => e.User_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

    }
}
