using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.DocumentId).HasColumnName("document_id").IsRequired();
        builder.Property(r => r.Reason).HasColumnName("reason").IsRequired();
        builder.Property(r => r.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(r => r.Status).HasColumnName("status").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        builder.Property(r => r.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(r => r.ResolvedBy).HasColumnName("resolved_by");
        builder.Property(r => r.ResolutionNote).HasColumnName("resolution_note").HasMaxLength(1000);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reports)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Document)
            .WithMany(d => d.Reports)
            .HasForeignKey(r => r.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ResolverUser)
            .WithMany(u => u.ResolvedReports)
            .HasForeignKey(r => r.ResolvedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
