using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class ViewConfiguration : IEntityTypeConfiguration<View>
{
    public void Configure(EntityTypeBuilder<View> builder)
    {
        builder.ToTable("views");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.UserId).HasColumnName("user_id");
        builder.Property(v => v.DocumentId).HasColumnName("document_id").IsRequired();
        builder.Property(v => v.IpAddress).HasColumnName("ip_address").HasColumnType("inet");
        builder.Property(v => v.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        builder.Property(v => v.ViewedAt).HasColumnName("viewed_at").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(v => v.User)
            .WithMany(u => u.Views)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.Document)
            .WithMany(d => d.Views)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
