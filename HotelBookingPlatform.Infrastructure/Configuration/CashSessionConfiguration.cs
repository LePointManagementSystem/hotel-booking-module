using HotelBookingPlatform.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingPlatform.Infrastructure.Configuration;

public class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.HasKey(x => x.CashSessionId);

        builder.Property(x => x.OpenedByUserId).IsRequired();
        builder.Property(x => x.OpeningBalance).HasColumnType("numeric(18,2)");

        builder.Property(x => x.ClosingCounted).HasColumnType("numeric(18,2)");

        builder.HasMany(x => x.Transactions)
            .WithOne(t => t.CashSession)
            .HasForeignKey(t => t.CashSessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}