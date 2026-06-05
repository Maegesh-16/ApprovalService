using Microsoft.EntityFrameworkCore;
using ApprovalService.API.Models;

namespace ApprovalService.API.Data
{
    public class ApprovalDbContext : DbContext
    {
        public ApprovalDbContext(DbContextOptions<ApprovalDbContext> options) : base(options)
        {
        }

        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<ApprovalHistory> ApprovalHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApprovalRequest>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);
                entity.Property(e => e.EntityType).HasConversion<int>();
                entity.Property(e => e.ApprovalType).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
            });

            modelBuilder.Entity<ApprovalHistory>(entity =>
            {
                entity.HasKey(e => e.HistoryId);
                entity.Property(e => e.Action).HasConversion<int>();
                entity.HasOne(e => e.ApprovalRequest)
                      .WithMany(a => a.ApprovalHistories)
                      .HasForeignKey(e => e.ApprovalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
