using Microsoft.EntityFrameworkCore;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role { Name = "Admin", Description = "Quản trị viên hệ thống" },
                new Role { Name = "Moderator", Description = "Kiểm duyệt viên nội dung" },
                new Role { Name = "Member", Description = "Thành viên người dùng" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. EducationLevels
        if (!await context.EducationLevels.AnyAsync())
        {
            var educationLevels = new[]
            {
                new EducationLevel { Code = "THCS", Name = "Trung học cơ sở", Description = "Khối học sinh THCS", DisplayOrder = 1, Status = EntityStatus.Active },
                new EducationLevel { Code = "THPT", Name = "Trung học phổ thông", Description = "Khối học sinh THPT", DisplayOrder = 2, Status = EntityStatus.Active },
                new EducationLevel { Code = "CD", Name = "Cao đẳng", Description = "Sinh viên các trường Cao đẳng", DisplayOrder = 3, Status = EntityStatus.Active },
                new EducationLevel { Code = "DH", Name = "Đại học", Description = "Sinh viên các trường Đại học", DisplayOrder = 4, Status = EntityStatus.Active }
            };
            await context.EducationLevels.AddRangeAsync(educationLevels);
            await context.SaveChangesAsync();
        }

        // 3. DocumentTypes
        if (!await context.DocumentTypes.AnyAsync())
        {
            var documentTypes = new[]
            {
                new DocumentType { Code = "GIAO_TRINH", Name = "Giáo trình", Icon = "book", Status = EntityStatus.Active },
                new DocumentType { Code = "DE_CUONG", Name = "Đề cương", Icon = "file-text", Status = EntityStatus.Active },
                new DocumentType { Code = "BAI_TAP", Name = "Bài tập", Icon = "edit", Status = EntityStatus.Active },
                new DocumentType { Code = "BAI_GIANG", Name = "Bài giảng", Icon = "presentation", Status = EntityStatus.Active },
                new DocumentType { Code = "SLIDE", Name = "Slide", Icon = "sliders", Status = EntityStatus.Active },
                new DocumentType { Code = "DE_THI", Name = "Đề thi", Icon = "clipboard", Status = EntityStatus.Active },
                new DocumentType { Code = "DAP_AN", Name = "Đáp án", Icon = "check-square", Status = EntityStatus.Active },
                new DocumentType { Code = "TAI_LIEU_THAM_KHAO", Name = "Tài liệu tham khảo", Icon = "bookmark", Status = EntityStatus.Active },
                new DocumentType { Code = "LUAN_VAN", Name = "Luận văn", Icon = "award", Status = EntityStatus.Active },
                new DocumentType { Code = "DO_AN", Name = "Đồ án", Icon = "folder", Status = EntityStatus.Active },
                new DocumentType { Code = "BAO_CAO", Name = "Báo cáo", Icon = "file-bar-chart", Status = EntityStatus.Active },
                new DocumentType { Code = "KHAC", Name = "Khác", Icon = "file", Status = EntityStatus.Active }
            };
            await context.DocumentTypes.AddRangeAsync(documentTypes);
            await context.SaveChangesAsync();
        }

        // 4. AcademicYears
        if (!await context.AcademicYears.AnyAsync())
        {
            var academicYears = new[]
            {
                new AcademicYear { Name = "2023-2024", StartYear = 2023, EndYear = 2024, Status = EntityStatus.Active },
                new AcademicYear { Name = "2024-2025", StartYear = 2024, EndYear = 2025, Status = EntityStatus.Active },
                new AcademicYear { Name = "2025-2026", StartYear = 2025, EndYear = 2026, Status = EntityStatus.Active },
                new AcademicYear { Name = "2026-2027", StartYear = 2026, EndYear = 2027, Status = EntityStatus.Active }
            };
            await context.AcademicYears.AddRangeAsync(academicYears);
            await context.SaveChangesAsync();
        }

        // 5. Admin User
        User adminUser;
        if (!await context.Users.AnyAsync(u => u.Email == "admin@sharestudy.vn"))
        {
            adminUser = new User
            {
                Username = "admin",
                Email = "admin@sharestudy.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "Quản trị viên Hệ thống",
                AvatarUrl = "https://ui-avatars.com/api/?name=Admin&background=0D8ABC&color=fff",
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            await context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await context.SaveChangesAsync();
        }
        else
        {
            adminUser = await context.Users.FirstAsync(u => u.Email == "admin@sharestudy.vn");
        }

        // 6. Demo Education Hierarchy
        if (!await context.Schools.AnyAsync())
        {
            var dhLevel = await context.EducationLevels.FirstAsync(e => e.Code == "DH");

            var school1 = new School
            {
                EducationLevelId = dhLevel.Id,
                Name = "Đại học Đồng Tháp",
                Code = "DTHU",
                Address = "783 Phạm Hữu Lầu, Phường 6, TP. Cao Lãnh, Đồng Tháp",
                Website = "https://dthu.edu.vn",
                Description = "Trường Đại học đào tạo đa ngành tại khu vực ĐBSCL",
                Status = EntityStatus.Active
            };

            var school2 = new School
            {
                EducationLevelId = dhLevel.Id,
                Name = "Đại học Cần Thơ",
                Code = "CTU",
                Address = "Khu II, đường 3/2, P. Xuân Khánh, Q. Ninh Kiều, TP. Cần Thơ",
                Website = "https://ctu.edu.vn",
                Description = "Trường Đại học trọng điểm quốc gia tại miền Tây",
                Status = EntityStatus.Active
            };

            var school3 = new School
            {
                EducationLevelId = dhLevel.Id,
                Name = "Đại học Bách Khoa TP.HCM",
                Code = "HCMUT",
                Address = "268 Lý Thường Kiệt, Phường 14, Quận 10, TP. Hồ Chí Minh",
                Website = "https://hcmut.edu.vn",
                Description = "Trường Đại học kỹ thuật hàng đầu Việt Nam",
                Status = EntityStatus.Active
            };

            await context.Schools.AddRangeAsync(school1, school2, school3);
            await context.SaveChangesAsync();

            // Faculties
            var faculty1 = new Faculty { SchoolId = school1.Id, Name = "Khoa Công nghệ Thông tin", Code = "FIT_DTHU", Description = "Khoa CNTT Đại học Đồng Tháp", Status = EntityStatus.Active };
            var faculty2 = new Faculty { SchoolId = school2.Id, Name = "Khoa Công nghệ Thông tin & Truyền thông", Code = "FICT_CTU", Description = "Khoa CNTT&TT Đại học Cần Thơ", Status = EntityStatus.Active };
            var faculty3 = new Faculty { SchoolId = school3.Id, Name = "Khoa Khoa học & Kỹ thuật Máy tính", Code = "CSE_HCMUT", Description = "Khoa KH&KT Máy tính Bách Khoa", Status = EntityStatus.Active };

            await context.Faculties.AddRangeAsync(faculty1, faculty2, faculty3);
            await context.SaveChangesAsync();

            // Majors
            var major1 = new Major { FacultyId = faculty1.Id, Name = "Ngành Công nghệ thông tin", Code = "IT_DTHU", Description = "Chuyên ngành CNTT ứng dụng", Status = EntityStatus.Active };
            var major2 = new Major { FacultyId = faculty2.Id, Name = "Ngành Công nghệ thông tin", Code = "IT_CTU", Description = "Chuyên ngành CNTT chuẩn chất lượng cao", Status = EntityStatus.Active };
            var major3 = new Major { FacultyId = faculty3.Id, Name = "Ngành Khoa học Máy tính", Code = "CS_HCMUT", Description = "Chuyên ngành KHMT Bách Khoa", Status = EntityStatus.Active };

            await context.Majors.AddRangeAsync(major1, major2, major3);
            await context.SaveChangesAsync();

            // Đại học Subjects
            var universitySubjects = new[]
            {
                new Subject { MajorId = major1.Id, Name = "Cơ sở dữ liệu", Code = "CSDL101", Credits = 3, Description = "Kiến thức CSDL quan hệ, SQL và tối ưu truy vấn", Status = EntityStatus.Active },
                new Subject { MajorId = major1.Id, Name = "Cấu trúc dữ liệu và giải thuật", Code = "CTDLGT102", Credits = 4, Description = "Các giải thuật tìm kiếm, sắp xếp và cấu trúc dữ liệu cơ bản", Status = EntityStatus.Active },
                new Subject { MajorId = major1.Id, Name = "Lập trình C#", Code = "CSHARP_DH", Credits = 3, Description = "Lập trình ứng dụng C# .NET Core", Status = EntityStatus.Active },
                new Subject { MajorId = major1.Id, Name = "Lập trình hướng đối tượng", Code = "OOP103", Credits = 3, Description = "Các nguyên lý OOP: Đóng gói, Thừa kế, Đa hình, Trừu tượng", Status = EntityStatus.Active },
                new Subject { MajorId = major2.Id, Name = "Mạng máy tính", Code = "MMT201", Credits = 3, Description = "Mô hình OSI, TCP/IP và cấu hình mạng", Status = EntityStatus.Active },
                new Subject { MajorId = major2.Id, Name = "Công nghệ phần mềm", Code = "CNPM202", Credits = 3, Description = "Quy trình phát triển phần mềm Agile/Scrum, thiết kế hệ thống", Status = EntityStatus.Active },
                new Subject { MajorId = major2.Id, Name = "Phân tích thiết kế hệ thống", Code = "PTTKHT_DH", Credits = 3, Description = "Biểu đồ UML, Use Case và thiết kế kiến trúc hệ thống", Status = EntityStatus.Active },
                new Subject { MajorId = major3.Id, Name = "Trí tuệ nhân tạo", Code = "TTNT301", Credits = 3, Description = "Học máy, Mạng Nơ-ron và Xử lý ngôn ngữ tự nhiên", Status = EntityStatus.Active },
            };

            await context.Subjects.AddRangeAsync(universitySubjects);
            await context.SaveChangesAsync();
        }

        // Check & Seed THCS subjects independently
        var thcsLevel = await context.EducationLevels.FirstOrDefaultAsync(e => e.Code == "THCS");
        if (thcsLevel != null && !await context.Subjects.AnyAsync(s => s.EducationLevelId == thcsLevel.Id))
        {
            var thcsSubjects = new[]
            {
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Ngữ văn", Code = "VAN_THCS", Credits = 3, Description = "Môn Ngữ văn THCS chuẩn Bộ GD&ĐT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Toán", Code = "TOAN_THCS", Credits = 3, Description = "Môn Toán THCS chuẩn Bộ GD&ĐT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Ngoại ngữ 1", Code = "NN1_THCS", Credits = 3, Description = "Môn Ngoại ngữ 1 THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Giáo dục công dân", Code = "GDCD_THCS", Credits = 3, Description = "Môn GDCD THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Lịch sử và Địa lí", Code = "LS_DL_THCS", Credits = 3, Description = "Môn Lịch sử và Địa lí THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Khoa học tự nhiên", Code = "KHTN_THCS", Credits = 3, Description = "Môn Khoa học tự nhiên THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Công nghệ", Code = "CN_THCS", Credits = 3, Description = "Môn Công nghệ THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Tin học", Code = "TIN_THCS", Credits = 3, Description = "Môn Tin học THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Giáo dục thể chất", Code = "GDTC_THCS", Credits = 3, Description = "Môn Giáo dục thể chất THCS", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thcsLevel.Id, Name = "Nghệ thuật", Code = "NT_THCS", Credits = 3, Description = "Môn Nghệ thuật (Âm nhạc, Mỹ thuật) THCS", Status = EntityStatus.Active },
            };
            await context.Subjects.AddRangeAsync(thcsSubjects);
            await context.SaveChangesAsync();
        }

        // Check & Seed THPT subjects independently
        var thptLevel = await context.EducationLevels.FirstOrDefaultAsync(e => e.Code == "THPT");
        if (thptLevel != null && !await context.Subjects.AnyAsync(s => s.EducationLevelId == thptLevel.Id))
        {
            var thptSubjects = new[]
            {
                new Subject { EducationLevelId = thptLevel.Id, Name = "Ngữ văn", Code = "VAN_THPT", Credits = 3, Description = "Môn Ngữ văn THPT chuẩn Bộ GD&ĐT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Toán", Code = "TOAN_THPT", Credits = 3, Description = "Môn Toán THPT chuẩn Bộ GD&ĐT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Ngoại ngữ 1", Code = "NN1_THPT", Credits = 3, Description = "Môn Ngoại ngữ 1 THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Giáo dục thể chất", Code = "GDTC_THPT", Credits = 3, Description = "Môn Giáo dục thể chất THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Giáo dục quốc phòng và an ninh", Code = "GDQPAN_THPT", Credits = 3, Description = "Môn GDQPAN THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Vật lí", Code = "LY_THPT", Credits = 3, Description = "Môn Vật lí THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Hóa học", Code = "HOA_THPT", Credits = 3, Description = "Môn Hóa học THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Sinh học", Code = "SINH_THPT", Credits = 3, Description = "Môn Sinh học THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Lịch sử", Code = "LS_THPT", Credits = 3, Description = "Môn Lịch sử THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Địa lí", Code = "DL_THPT", Credits = 3, Description = "Môn Địa lí THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Giáo dục kinh tế và pháp luật", Code = "GDKTPL_THPT", Credits = 3, Description = "Môn GDKTPL THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Tin học", Code = "TIN_THPT", Credits = 3, Description = "Môn Tin học THPT", Status = EntityStatus.Active },
                new Subject { EducationLevelId = thptLevel.Id, Name = "Công nghệ", Code = "CN_THPT", Credits = 3, Description = "Môn Công nghệ THPT", Status = EntityStatus.Active },
            };
            await context.Subjects.AddRangeAsync(thptSubjects);
            await context.SaveChangesAsync();
        }

        // 7. Demo Documents
        if (!await context.Documents.AnyAsync())
        {
            var dhLevel = await context.EducationLevels.FirstAsync(e => e.Code == "DH");
            var school = await context.Schools.FirstAsync(s => s.Code == "DTHU");
            var faculty = await context.Faculties.FirstAsync(f => f.Code == "FIT_DTHU");
            var major = await context.Majors.FirstAsync(m => m.Code == "IT_DTHU");
            var subjectsList = await context.Subjects.ToListAsync();
            var docTypes = await context.DocumentTypes.ToListAsync();
            var academicYear = await context.AcademicYears.FirstAsync(a => a.Name == "2024-2025");

            var csdlSub = subjectsList.FirstOrDefault(s => s.Code == "CSDL101") ?? subjectsList.First();
            var csharpSub = subjectsList.FirstOrDefault(s => s.Code == "CSHARP_DH") ?? subjectsList.First();

            var doc1 = new Document
            {
                EducationLevelId = dhLevel.Id,
                SchoolId = school.Id,
                FacultyId = faculty.Id,
                MajorId = major.Id,
                SubjectId = csdlSub.Id,
                DocumentTypeId = docTypes.First(d => d.Code == "GIAO_TRINH").Id,
                AcademicYearId = academicYear.Id,
                UserId = adminUser.Id,
                Title = "Giáo trình Cơ sở dữ liệu nâng cao 2024",
                Slug = "giao-trinh-co-so-du-lieu-nang-cao-2024",
                Description = "Tài liệu bài giảng chi tiết về thiết kế cơ sở dữ liệu quan hệ, các dạng chuẩn hóa 1NF, 2NF, 3NF, BCNF và tối ưu hóa câu lệnh SQL trong SQL Server.",
                FileUrl = "/uploads/documents/sample_csdl.pdf",
                ThumbnailUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&q=80",
                FileType = "pdf",
                FileSize = 5242880, // 5MB
                ViewCount = 1250,
                DownloadCount = 430,
                Status = DocumentStatus.Approved,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            };

            var doc2 = new Document
            {
                EducationLevelId = dhLevel.Id,
                SchoolId = school.Id,
                FacultyId = faculty.Id,
                MajorId = major.Id,
                SubjectId = csharpSub.Id,
                DocumentTypeId = docTypes.First(d => d.Code == "DE_THI").Id,
                AcademicYearId = academicYear.Id,
                UserId = adminUser.Id,
                Title = "Đề thi giữa kỳ môn Lập trình C# .NET Core",
                Slug = "de-thi-giua-ky-mon-lap-trinh-csharp-net-core",
                Description = "Đề thi viết và thực hành trên máy tính môn Lập trình C# kèm đáp án chi tiết cho sinh viên khối ngành CNTT.",
                FileUrl = "/uploads/documents/sample_csharp.pdf",
                ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=400&q=80",
                FileType = "pdf",
                FileSize = 2097152, // 2MB
                ViewCount = 890,
                DownloadCount = 310,
                Status = DocumentStatus.Approved,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            };

            await context.Documents.AddRangeAsync(doc1, doc2);
            await context.SaveChangesAsync();
        }

        // 13. System Settings Seed
        if (!await context.SystemSettings.AnyAsync())
        {
            var defaultSettings = new List<SystemSetting>
            {
                new() { Key = "SiteName", Value = "ShareStudy - Hệ thống Chia sẻ Tài liệu Học tập", Description = "Tên chính thức trang web", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "SiteSlogan", Value = "Tri thức mở, kết nối cộng đồng học thuật Việt Nam", Description = "Khẩu hiệu hiển thị", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "ContactEmail", Value = "support@sharestudy.vn", Description = "Email hỗ trợ", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "MaintenanceMode", Value = "false", Description = "Trạng thái bảo trì", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "AllowRegistration", Value = "true", Description = "Cho phép đăng ký", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "RequireEmailVerification", Value = "true", Description = "Xác thực email", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "MaxUploadMB", Value = "50", Description = "Dung lượng upload tối đa", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "AllowedFileExtensions", Value = "pdf,docx,pptx,xlsx,zip", Description = "Định dạng cho phép", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "NotifyOnDocumentApproval", Value = "true", Description = "Thông báo duyệt bài", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "NotifyOnNewReport", Value = "true", Description = "Thông báo báo cáo", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "SmtpHost", Value = "smtp.gmail.com", Description = "Máy chủ SMTP", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "SmtpPort", Value = "587", Description = "Cổng SMTP", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "SmtpEmail", Value = "no-reply@sharestudy.vn", Description = "Email gửi tin", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "StorageProvider", Value = "Local", Description = "Phương thức lưu trữ", CreatedAt = DateTimeOffset.UtcNow },
                new() { Key = "StorageBasePath", Value = "./wwwroot/uploads", Description = "Đường dẫn lưu trữ", CreatedAt = DateTimeOffset.UtcNow }
            };
            await context.SystemSettings.AddRangeAsync(defaultSettings);
            await context.SaveChangesAsync();
        }

        // 14. Audit Logs Seed
        if (!await context.AuditLogs.AnyAsync())
        {
            var initialLogs = new List<AuditLog>
            {
                new() {
                    UserId = adminUser.Id,
                    Performer = "Quản trị viên Hệ thống (admin)",
                    IpAddress = "192.168.1.1",
                    ActionType = "UPDATE_ROLE",
                    ActionName = "Cập nhật vai trò người dùng",
                    Module = "Users",
                    Severity = "INFO",
                    Details = "Đã gán vai trò Kiểm duyệt viên (Moderator) cho tài khoản @mcv",
                    Payload = "{\"userId\":4,\"newRoles\":[\"Moderator\",\"Member\"],\"performedBy\":\"admin\"}",
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
                },
                new() {
                    UserId = adminUser.Id,
                    Performer = "Mai Chí Vĩnh (mcv)",
                    IpAddress = "118.69.182.45",
                    ActionType = "APPROVE_DOCUMENT",
                    ActionName = "Phê duyệt tài liệu",
                    Module = "Documents",
                    Severity = "SUCCESS",
                    Details = "Đã phê duyệt tài liệu: Giáo trình Cơ sở dữ liệu nâng cao 2024",
                    Payload = "{\"documentId\":1,\"status\":\"Approved\"}",
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                },
                new() {
                    UserId = adminUser.Id,
                    Performer = "Quản trị viên Hệ thống (admin)",
                    IpAddress = "192.168.1.1",
                    ActionType = "CREATE_SUBJECT",
                    ActionName = "Thêm mới môn học",
                    Module = "Education",
                    Severity = "SUCCESS",
                    Details = "Đã thêm mới môn học Khoa học tự nhiên thuộc Cấp THCS",
                    Payload = "{\"code\":\"KHTN_THCS\",\"name\":\"Khoa học tự nhiên\"}",
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-3)
                }
            };
            await context.AuditLogs.AddRangeAsync(initialLogs);
            await context.SaveChangesAsync();
        }
    }
}
