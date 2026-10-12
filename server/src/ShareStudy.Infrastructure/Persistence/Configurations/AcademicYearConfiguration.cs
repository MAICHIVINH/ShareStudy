using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("academic_years");

        builder.HasKey(ay => ay.Id);
        builder.Property(ay => ay.Id).HasColumnName("id");
        builder.Property(ay => ay.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(ay => ay.StartYear).HasColumnName("start_year").IsRequired();
        builder.Property(ay => ay.EndYear).HasColumnName("end_year").IsRequired();
        builder.Property(ay => ay.Status).HasColumnName("status").IsRequired();
        builder.Property(ay => ay.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ay => ay.UpdatedAt).HasColumnName("updated_at");
    }
}
