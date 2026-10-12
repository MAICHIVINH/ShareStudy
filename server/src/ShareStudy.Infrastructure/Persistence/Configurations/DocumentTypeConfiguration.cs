using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("document_types");

        builder.HasKey(dt => dt.Id);
        builder.Property(dt => dt.Id).HasColumnName("id");
        builder.Property(dt => dt.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(dt => dt.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(dt => dt.Icon).HasColumnName("icon").HasMaxLength(100);
        builder.Property(dt => dt.Status).HasColumnName("status").IsRequired();
        builder.Property(dt => dt.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(dt => dt.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(dt => dt.Code).IsUnique().HasDatabaseName("ux_document_types_code");
    }
}
