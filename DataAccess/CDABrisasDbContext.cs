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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("UserWhatsapp");
                entity.HasKey(u => u.Id);

                entity.HasOne(u => u.Messages).WithOne(m => m.User).HasForeignKey<Message>(a => a.UserId);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");
                entity.HasKey(u => u.Id);
                //entity.HasOne(u => u.PaymentStatus).WithOne(m => m.Messages).HasForeignKey<Message>(a => a.PaymentStatusId);
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

            modelBuilder.Entity<PaymentStatus>()
        .HasMany(p => p.Messages)
        .WithOne(m => m.PaymentStatus)
        .HasForeignKey(m => m.PaymentStatusId);
        }
    }
}
