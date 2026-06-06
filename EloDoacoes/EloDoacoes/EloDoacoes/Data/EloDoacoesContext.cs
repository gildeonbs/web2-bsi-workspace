using EloDoacoes.Models;
using Microsoft.EntityFrameworkCore;

namespace EloDoacoes.Data
{
    public class EloDoacoesContext : DbContext
    {
        public EloDoacoesContext(DbContextOptions<EloDoacoesContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationStatus> ReservationsStatuses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<DonationStatus> DonationStatuses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<DonationImage> DonationImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Donation>().ToTable("donation");
            modelBuilder.Entity<Reservation>().ToTable("reservation");
            modelBuilder.Entity<ReservationStatus>().ToTable("reservation_status");
            modelBuilder.Entity<Category>().ToTable("category");
            modelBuilder.Entity<DonationStatus>().ToTable("donation_status");
            modelBuilder.Entity<Role>().ToTable("role");
            modelBuilder.Entity<DonationImage>().ToTable("donation_image");
        }
    }
}
