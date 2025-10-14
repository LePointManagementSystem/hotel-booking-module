namespace HotelBookingPlatform.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<LocalUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> option) : base(option) { }

        public AppDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hotel>()
            .HasOne(h => h.Owner)
            .WithMany(o => o.Hotels)
            .HasForeignKey(h => h.OwnerID)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Room>()
            .Property(r => r.RoomID)
            .ValueGeneratedOnAdd(); // ✅ EF sait que RoomID est auto-généré


        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.CreatedBy)
            .WithMany()
            .HasForeignKey(b => b.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany(g => g.Bookings)
                .HasForeignKey(b => b.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasOne(s => s.Hotel)
                .WithMany()
                .HasForeignKey(s => s.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.ApplyConfiguration(new HotelConfiguration());
            modelBuilder.ApplyConfiguration(new RoomClassConfiguration());
            modelBuilder.ApplyConfiguration(new RoomConfiguration());
            modelBuilder.ApplyConfiguration(new BookingConfiguration());
            modelBuilder.ApplyConfiguration(new DiscountConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewConfiguration());
            modelBuilder.ApplyConfiguration(new StaffConfiguration());
        }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }
        public DbSet<Guest> Guests {get; set; }
        public DbSet<RoomClass> RoomClasses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<InvoiceRecord> InvoiceRecords { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Staff> Staff { get; set; }
    }
}