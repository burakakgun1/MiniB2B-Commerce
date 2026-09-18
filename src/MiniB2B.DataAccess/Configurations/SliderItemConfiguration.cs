using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Core.Entities;

namespace MiniB2B.DataAccess.Configurations;

public class SliderItemConfiguration : IEntityTypeConfiguration<SliderItem>
{
    public void Configure(EntityTypeBuilder<SliderItem> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Subtitle)
            .HasMaxLength(250);

        builder.Property(s => s.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.TargetUrl)
            .HasMaxLength(500);
    }
}
