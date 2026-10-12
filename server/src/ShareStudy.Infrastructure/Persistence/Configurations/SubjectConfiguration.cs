using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.EducationLevelId).HasColumnName("education_level_id");
        builder.Property(s => s.MajorId).HasColumnName("major_id");
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Credits).HasColumnName("credits").IsRequired();
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(s => s.Status).HasColumnName("status").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(s => s.EducationLevel)
            .WithMany()
            .HasForeignKey(s => s.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Major)
            .WithMany(m => m.Subjects)
            .HasForeignKey(s => s.MajorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
