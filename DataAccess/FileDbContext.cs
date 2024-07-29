using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DataAccess
{
    public class FileDbContext : DbContext
    {

        public DbSet<DTRequestTable> HCPOSDownloadRequest { get; set; }
        public DbSet<DTFilesTable> HCPOSDownloadFiles { get; set; }
        public DbSet<DTVendorPortalConfig> VendorPortalConfig { get; set; }
        public DbSet<DTDownloadQueue> HCPOSDownloadQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DTFilesTable>()
                .HasOne(f => f.Request)
                .WithMany(r => r.Files)
                .HasForeignKey(f => f.RequestId);

            modelBuilder.Entity<DTVendorPortalConfig>()
            .HasKey(c => c.CountryCode);

            modelBuilder.Entity<DTRequestTable>()
                   .HasOne(r => r.DownloadQueue)
                   .WithMany()
                   .HasForeignKey(r => r.QueueID);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=HCUSSB961;Database=wPasta;Integrated Security=True; TrustServerCertificate = True");
            optionsBuilder.EnableSensitiveDataLogging();

        }

    }
}
