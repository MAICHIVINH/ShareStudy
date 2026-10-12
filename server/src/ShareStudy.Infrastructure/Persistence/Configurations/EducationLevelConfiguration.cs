using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class EducationLevelConfiguration : IEntityTypeConfiguration<EducationLevel>
{
    public void Configure(EntityTypeBuilder<EducationLevel> builder)
    {
        builder.ToTable("education_levels");

        builder.HasKey(el => el.Id);
        builder.Property(el => el.Id).HasColumnName("id");
        builder.Property(el => el.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(el => el.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(el => el.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(el => el.DisplayOrder).HasColumnName("display_order").IsRequired();
        builder.Property(el => el.Status).HasColumnName("status").IsRequired();
        builder.Property(el => el.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(el => el.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(el => el.Code).IsUnique().HasDatabaseName("ux_education_levels_code");
    }
}
