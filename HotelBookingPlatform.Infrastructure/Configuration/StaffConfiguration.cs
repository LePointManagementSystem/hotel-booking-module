using HotelBookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingPlatform.Infrastructure.Configuration
{
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            builder.HasKey(s => s.StaffId);
            builder.Property(s => s.StaffId).ValueGeneratedOnAdd();

            builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Role).IsRequired().HasMaxLength(50);
            builder.Property(s => s.Email).HasMaxLength(100);
            builder.Property(s => s.PhoneNumber).HasMaxLength(20);
            builder.Property(s => s.CreatedAtUtc).IsRequired();

            builder.HasOne(s => s.Hotel)
                    .WithMany(h => h.StaffMembers)
                    .HasForeignKey(s => s.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

            builder.ToTable("Staff");
            builder.HasIndex(s => new { s.HotelId, s.IsActive });
            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.Email);
        }
    }
}