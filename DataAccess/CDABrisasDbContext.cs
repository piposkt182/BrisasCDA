using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class CDABrisasDbContext : DbContext
    {
        public CDABrisasDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<PaymentStatus> PaymentsStatus { get; set; }
        public DbSet<Agreement> Agreements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Customer");
                entity.HasKey(u => u.Id);

                modelBuilder.Entity<User>()
                            .HasMany(u => u.Messages)
                            .WithOne(m => m.User)
                            .HasForeignKey(m => m.UserId)
                            .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");
                entity.HasKey(u => u.Id);
            });

            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.ToTable("SystemUser");
                entity.HasKey(u => u.Id);
            });

            modelBuilder.Entity<PaymentStatus>(entity =>
            {
                entity.ToTable("PaymentStatus");
                entity.HasKey(u => u.Id);
            });

            modelBuilder.Entity<Agreement>(entity =>
            {
                entity.ToTable("Agreement");
                entity.HasKey(a => a.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<PaymentStatus>()
                        .HasMany(p => p.Messages)
                        .WithOne(m => m.PaymentStatus)
                        .HasForeignKey(m => m.PaymentStatusId);

            modelBuilder.Entity<Agreement>()
                       .HasMany(p => p.Messages)
                       .WithOne(m => m.Agreements)
                       .HasForeignKey(m => m.AgreementId);
        }
    }
}
