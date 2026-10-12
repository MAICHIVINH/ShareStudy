using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class MajorConfiguration : IEntityTypeConfiguration<Major>
{
    public void Configure(EntityTypeBuilder<Major> builder)
    {
        builder.ToTable("majors");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.FacultyId).HasColumnName("faculty_id").IsRequired();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(m => m.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(m => m.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(m => m.Status).HasColumnName("status").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(m => m.Faculty)
            .WithMany(f => f.Majors)
            .HasForeignKey(m => m.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
