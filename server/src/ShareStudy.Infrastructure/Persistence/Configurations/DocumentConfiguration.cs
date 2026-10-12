using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(d => d.EducationLevelId).HasColumnName("education_level_id").IsRequired();
        builder.Property(d => d.SchoolId).HasColumnName("school_id");
        builder.Property(d => d.FacultyId).HasColumnName("faculty_id");
        builder.Property(d => d.MajorId).HasColumnName("major_id");
        builder.Property(d => d.SubjectId).HasColumnName("subject_id");
        builder.Property(d => d.DocumentTypeId).HasColumnName("document_type_id").IsRequired();
        builder.Property(d => d.AcademicYearId).HasColumnName("academic_year_id");

        builder.Property(d => d.Title).HasColumnName("title").HasMaxLength(250).IsRequired();
        builder.Property(d => d.Slug).HasColumnName("slug").HasMaxLength(300).IsRequired();
        builder.Property(d => d.Description).HasColumnName("description").HasMaxLength(4000);

        builder.Property(d => d.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(d => d.StoredFileName).HasColumnName("stored_file_name").HasMaxLength(255).IsRequired();
        builder.Property(d => d.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
        builder.Property(d => d.FileType).HasColumnName("file_type").HasMaxLength(50).IsRequired();
        builder.Property(d => d.MimeType).HasColumnName("mime_type").HasMaxLength(100).IsRequired();
        builder.Property(d => d.FileSize).HasColumnName("file_size").IsRequired();

        builder.Property(d => d.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500);

        builder.Property(d => d.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
        builder.Property(d => d.DownloadCount).HasColumnName("download_count").HasDefaultValue(0);
        builder.Property(d => d.FavoriteCount).HasColumnName("favorite_count").HasDefaultValue(0);
        builder.Property(d => d.AverageRating).HasColumnName("average_rating").HasColumnType("numeric(3,2)").HasDefaultValue(0);
        builder.Property(d => d.RatingCount).HasColumnName("rating_count").HasDefaultValue(0);

        builder.Property(d => d.Status).HasColumnName("status").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
        builder.Property(d => d.PublishedAt).HasColumnName("published_at");

        // Unique constraint
        builder.HasIndex(d => d.Slug).IsUnique().HasDatabaseName("ux_documents_slug");

        // Specific indexes required by spec
        builder.HasIndex(d => d.SubjectId).HasDatabaseName("idx_documents_subject");
        builder.HasIndex(d => d.MajorId).HasDatabaseName("idx_documents_major");
        builder.HasIndex(d => d.SchoolId).HasDatabaseName("idx_documents_school");
        builder.HasIndex(d => d.FacultyId).HasDatabaseName("idx_documents_faculty");
        builder.HasIndex(d => d.EducationLevelId).HasDatabaseName("idx_documents_education_level");
        builder.HasIndex(d => d.DocumentTypeId).HasDatabaseName("idx_documents_document_type");
        builder.HasIndex(d => d.Status).HasDatabaseName("idx_documents_status");
        builder.HasIndex(d => d.CreatedAt).IsDescending().HasDatabaseName("idx_documents_created_at");
        builder.HasIndex(d => d.DownloadCount).HasDatabaseName("idx_documents_download_count");
        builder.HasIndex(d => d.ViewCount).HasDatabaseName("idx_documents_view_count");

        // Full-Text Search GIN index
        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnName("search_vector")
            .HasComputedColumnSql("to_tsvector('simple', coalesce(title, '') || ' ' || coalesce(description, ''))", stored: true);

        builder.HasIndex("SearchVector")
            .HasMethod("gin")
            .HasDatabaseName("idx_documents_search_vector");

        // Relationships
        builder.HasOne(d => d.User)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.EducationLevel)
            .WithMany(el => el.Documents)
            .HasForeignKey(d => d.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.School)
            .WithMany(s => s.Documents)
            .HasForeignKey(d => d.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Faculty)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Major)
            .WithMany(m => m.Documents)
            .HasForeignKey(d => d.MajorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Subject)
            .WithMany(s => s.Documents)
            .HasForeignKey(d => d.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.DocumentType)
            .WithMany(dt => dt.Documents)
            .HasForeignKey(d => d.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.AcademicYear)
            .WithMany(ay => ay.Documents)
            .HasForeignKey(d => d.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
