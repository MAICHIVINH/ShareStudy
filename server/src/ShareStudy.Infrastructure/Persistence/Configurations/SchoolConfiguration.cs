using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("schools");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.EducationLevelId).HasColumnName("education_level_id").IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Address).HasColumnName("address").HasMaxLength(300);
        builder.Property(s => s.Website).HasColumnName("website").HasMaxLength(200);
        builder.Property(s => s.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(s => s.Status).HasColumnName("status").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(s => s.EducationLevel)
            .WithMany(el => el.Schools)
            .HasForeignKey(s => s.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
