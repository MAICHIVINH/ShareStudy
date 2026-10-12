using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("faculties");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.SchoolId).HasColumnName("school_id").IsRequired();
        builder.Property(f => f.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(f => f.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(f => f.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(f => f.Status).HasColumnName("status").IsRequired();
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(f => f.School)
            .WithMany(s => s.Faculties)
            .HasForeignKey(f => f.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
