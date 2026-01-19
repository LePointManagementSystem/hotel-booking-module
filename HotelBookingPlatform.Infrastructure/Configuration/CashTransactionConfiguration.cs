using HotelBookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingPlatform.Infrastructure.Configuration;

public class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.ToTable("CashTransactions");

        builder.HasKey(x => x.CashTransactionID);

        builder.Property(x => x.Amount)
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Shift)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(100);

        builder.Property(x => x.Reference)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.HotelId, x.CreatedAtUtc });
    }
}
