using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Core.Entities;

namespace MiniB2B.DataAccess.Configurations;

public class GridColumnConfigConfiguration : IEntityTypeConfiguration<GridColumnConfig>
{
    public void Configure(EntityTypeBuilder<GridColumnConfig> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.GridName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.PropertyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.HeaderTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Width)
            .HasMaxLength(50);

        builder.Property(g => g.Alignment)
            .HasMaxLength(20);

        builder.HasIndex(g => new { g.GridName, g.PropertyName })
            .IsUnique();
    }
}
