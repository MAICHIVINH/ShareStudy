using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class DownloadHistoryConfiguration : IEntityTypeConfiguration<DownloadHistory>
{
    public void Configure(EntityTypeBuilder<DownloadHistory> builder)
    {
        builder.ToTable("download_histories");

        builder.HasKey(dh => dh.Id);
        builder.Property(dh => dh.Id).HasColumnName("id");
        builder.Property(dh => dh.UserId).HasColumnName("user_id");
        builder.Property(dh => dh.DocumentId).HasColumnName("document_id").IsRequired();
        builder.Property(dh => dh.IpAddress).HasColumnName("ip_address").HasColumnType("inet");
        builder.Property(dh => dh.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        builder.Property(dh => dh.DownloadedAt).HasColumnName("downloaded_at").IsRequired();
        builder.Property(dh => dh.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(dh => dh.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(dh => dh.User)
            .WithMany(u => u.DownloadHistories)
            .HasForeignKey(dh => dh.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(dh => dh.Document)
            .WithMany(d => d.DownloadHistories)
            .HasForeignKey(dh => dh.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
