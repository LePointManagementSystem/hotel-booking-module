using HotelBookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingPlatform.Infrastructure.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.NotificationId);

            builder.Property(n => n.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(n => n.Message)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(n => n.Type)
                .IsRequired();

            builder.Property(n => n.CreatedAtUtc)
                .IsRequired();

            builder.Property(n => n.IsRead)
                .IsRequired();

            // Index utiles
            builder.HasIndex(n => n.HotelId);
            builder.HasIndex(n => new { n.HotelId, n.IsRead });
            builder.HasIndex(n => n.CreatedAtUtc);

            // ✅ FK optionnelle BookingId sans navigation
            builder.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(n => n.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ FK optionnelle RoomId sans navigation
            builder.HasOne<Room>()
                .WithMany()
                .HasForeignKey(n => n.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // (Optionnel) Si tu as RecipientUserId
            // builder.HasIndex(n => n.RecipientUserId);
        }
    }
}
