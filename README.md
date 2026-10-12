<div align="center">

# ShareStudy — Nền tảng Chia sẻ & Quản lý Tài liệu Học tập

**Hệ sinh thái Web Application toàn diện hỗ trợ học sinh, sinh viên và giảng viên lưu trữ, chia sẻ, tìm kiếm, đọc trước và thảo luận tài liệu học tập.**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-8.x-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![MinIO](https://img.shields.io/badge/MinIO-S3_Compatible-C72C48?logo=minio&logoColor=white)](https://min.io/)
[![Cloudflare R2](https://img.shields.io/badge/Cloudflare-R2_Storage-F38020?logo=cloudflare&logoColor=white)](https://www.cloudflare.com/products/r2/)
[![Ant Design](https://img.shields.io/badge/Ant%20Design-v6-0170FE?logo=antdesign&logoColor=white)](https://ant.design/)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-v3.4-38B2AC?logo=tailwindcss&logoColor=white)](https://tailwindcss.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

[Giới thiệu](#1-giới-thiệu-tổng-quan) • [Kiến trúc](#2-kiến-trúc-hệ-thống) • [Tính năng](#3-tính-năng-nổi-bật) • [Cấu trúc dự án](#4-cấu-trúc-thư-mục-dự-án) • [Biến môi trường](#5-cấu-hình-biến-môi-trường-env) • [Khởi chạy Local](#6-hướng-dẫn-khởi-chạy-môi-trường-local) • [Tài khoản mẫu](#7-tài-khoản-đăng-nhập-mặc-định) • [REST API](#8-tổng-quan-api-endpoints) • [Triển khai Cloud](#9-triển-khai-production-cloud) • [Bảo mật](#10-bảo-mật--an-toàn-thông-tin)

---

</div>

## 1. Giới thiệu Tổng quan

**ShareStudy** là một nền tảng mã nguồn mở được xây dựng nhằm giải quyết bài toán phân tán và thất lạc tài liệu trong môi trường giáo dục tại Việt Nam. Dự án cung cấp kho học liệu có cấu trúc phân tầng rõ ràng từ phổ thông (**THCS / THPT**) đến đại học (**Đại học / Cao đẳng / Học viện**), hỗ trợ tìm kiếm ngữ nghĩa, xem trước file tài liệu trực tiếp trên trình duyệt (PDF Viewer), chấm điểm đánh giá và thảo luận nhiều cấp.

### Điểm nổi bật:
* **Kiến trúc Clean Architecture**: Phân tách độc lập tầng Domain, Application, Infrastructure và Web API giúp mã nguồn dễ bảo trì, mở rộng và kiểm thử.
* **Lưu trữ Object Storage linh hoạt**: Tích hợp MinIO cho môi trường Local Development và tự động chuyển đổi sang Cloudflare R2 trên Production mà không cần đổi code.
* **Bảo mật chuẩn Enterprise**: Xác thực JWT token, phân quyền Role-based, ghi nhật ký kiểm toán (Audit Logs) và cơ chế Chế độ bảo trì hệ thống (Maintenance Mode).
* **Trải nghiệm người dùng cao cấp**: Giao diện Ant Design hiện đại kết hợp Tailwind CSS, hỗ trợ chuyển đổi chủ đề Sáng / Tối (Dark Mode) tức thì.

---

## 2. Kiến trúc Hệ thống

### A. Mô hình Phát triển Cục bộ (Local Development)
```text
  +-----------------------------------------------------------------------------------+
  |                             LOCAL DEVELOPMENT SYSTEM                              |
  +-----------------------------------------------------------------------------------+
                                            |
            +-------------------------------+-------------------------------+
            |                                                               |
            v                                                               v
  +----------------------------------+             +----------------------------------+
  |  Frontend Client (React 19 Vite) |             |  Backend Web API (ASP.NET Core 8)|
  |  http://localhost:5173           | --- HTTP -> |  http://localhost:5000           |
  |  • Ant Design v6 + Tailwind CSS  |             |  • Clean Architecture            |
  |  • Axios + React Router v7       |             |  • Swagger / OpenAPI v1          |
  +----------------------------------+             +----------------------------------+
                                                                    |
                                        +---------------------------+---------------------------+
                                        |                                                       |
                                        v                                                       v
                      +-----------------------------------+                   +-----------------------------------+
                      |   PostgreSQL 16 Database          |                   |   MinIO Object Storage            |
                      |   localhost:5432                  |                   |   localhost:9000 (S3 API)         |
                      |   • Quản lý qua Docker Compose    |                   |   localhost:9001 (Web Console)    |
                      +-----------------------------------+                   +-----------------------------------+
```

---

### B. Mô hình Triển khai Đám mây (Production Cloud)
```text
  +-----------------------------------------------------------------------------------+
  |                             PRODUCTION CLOUD SYSTEM                               |
  +-----------------------------------------------------------------------------------+
                                            |
            +-------------------------------+-------------------------------+
            |                                                               |
            v                                                               v
  +----------------------------------+             +----------------------------------+
  |  VERCEL (Frontend SPA)           |             |  RENDER (Backend API Web Service)|
  |  https://sharestudy.vercel.app   | --- HTTPS-> |  https://sharestudy-api.onrender.com
  |  • Static Asset CDN & Edge Cache |             |  • Containerized Docker Runtime  |
  +----------------------------------+             +----------------------------------+
                                                                    |
                                        +---------------------------+---------------------------+
                                        |                                                       |
                                        v                                                       v
                      +-----------------------------------+                   +-----------------------------------+
                      |   NEON TECH (Serverless Postgres) |                   |   CLOUDFLARE R2                   |
                      |   • Pooled Connection / SSL       |                   |   • S3-Compatible Object Storage  |
                      |   • Auto Scaling & Daily Backup   |                   |   • Không phí băng thông tải về   |
                      +-----------------------------------+                   +-----------------------------------+
```

---

### Công nghệ & Thư viện Sử dụng

| Phân hệ | Công nghệ / Thư viện | Vai trò & Mục đích sử dụng |
|---|---|---|
| **Backend** | **ASP.NET Core 8 Web API** | Framework chính xử lý RESTful API hiệu năng cao |
| | **Entity Framework Core 8** | ORM truy xuất CSDL PostgreSQL, quản lý Migrations |
| | **Npgsql.EntityFrameworkCore.PostgreSQL** | Database Driver tối ưu cho PostgreSQL |
| | **MinIO .NET SDK / AWSSDK.S3** | Tương tác lưu trữ file với MinIO (Local) và Cloudflare R2 (Prod) |
| | **Microsoft.AspNetCore.Authentication.JwtBearer** | Xác thực người dùng qua JWT Access Token |
| | **Swashbuckle.AspNetCore** | Sinh tài liệu API Swagger/OpenAPI tự động |
| **Frontend** | **React 19 & Vite 8** | Nền tảng xây dựng Single Page Application hiện đại |
| | **Ant Design v6** | Hệ thống UI Components chuẩn doanh nghiệp |
| | **Tailwind CSS v3.4** | Utility-first CSS xây dựng layout và responsive linh hoạt |
| | **React Router v7** | Quản lý điều hướng, chuyển trang và Protected Routes |
| | **Recharts** | Biểu đồ trực quan hóa dữ liệu thống kê cho trang Dashboard |
| | **react-hot-toast** | Quản lý thông báo toast notification người dùng |
| | **Axios** | HTTP Client gửi request kèm Interceptor xử lý Bearer Token |
| **DevOps & DB** | **PostgreSQL 16** | Hệ quản trị cơ sở dữ liệu quan hệ chính |
| | **MinIO / Cloudflare R2** | Lưu trữ tài liệu (PDF, Word, Slide, hình ảnh) |
| | **Docker & Docker Compose** | Đóng gói môi trường chạy DB và MinIO đồng nhất |
| | **Render & Vercel** | Nền tảng Cloud Hosting triển khai tự động qua Git |

---

## 3. Tính năng Nổi bật

### Dành cho Học viên & Sinh viên (Members & Guests)
* **Tìm kiếm đa tầng & Bộ lọc thông minh**: Tìm theo từ khóa, Cấp học, Trường học, Khoa, Ngành, Môn học, Năm học, Định dạng file và Loại tài liệu (Đề thi, Giáo trình, Đồ án,...).
* **Trình đọc PDF trực tiếp (In-browser Viewer)**: Xem nội dung tài liệu trước khi quyết định tải về mà không cần phần mềm thứ ba.
* **Tải xuống an toàn**: Tự động sinh Presigned URL tải file, tự động cập nhật số lượt xem và lượt tải về.
* **Đánh giá & Bình luận phân cấp**: Chấm điểm (1 - 5 sao), thảo luận theo cây phân cấp (hỗ trợ Trả lời, Chỉnh sửa và Xóa bình luận cá nhân).
* **Tài liệu Yêu thích**: Lưu lại các tài liệu quan trọng vào danh sách cá nhân để xem lại nhanh chóng.
* **Báo cáo vi phạm**: Gửi yêu cầu kiểm duyệt đối với tài liệu có nội dung không phù hợp hoặc vi phạm bản quyền.

### Dành cho Kiểm duyệt viên (Moderators)
* **Hàng đợi Phê duyệt tài liệu**: Duyệt xem nội dung chi tiết trước khi công khai ra cộng đồng; hỗ trợ Duyệt (`Approved`), Từ chối (`Rejected`) hoặc Ẩn (`Hidden`).
* **Xử lý Báo cáo vi phạm**: Xem danh sách báo cáo từ thành viên, ra quyết định xử lý hoặc gỡ bỏ tài liệu sai quy định.

### Dành cho Quản trị viên Hệ thống (System Admins)
* **Thống kê Tổng quan (Dashboard)**: Biểu đồ đường và cột trực quan về số tài liệu mới, lượt tải, số thành viên mới theo mốc thời gian.
* **Quản lý Tài khoản & Phân quyền**: Quản lý danh sách người dùng, kích hoạt/khóa tài khoản, gán vai trò (`Admin`, `Moderator`, `Member`).
* **Quản lý Danh mục Giáo dục**: CRUD toàn bộ cấu trúc: Cấp học $\rightarrow$ Trường học $\rightarrow$ Khoa $\rightarrow$ Ngành $\rightarrow$ Môn học $\rightarrow$ Năm học.
* **Nhật ký Hệ thống (Audit Logs)**: Ghi lại đầy đủ mọi tác động hệ thống (Thêm, Sửa, Xóa) kèm thông tin thời gian, người thực hiện và IP client.
* **Cài đặt Hệ thống (System Settings)**: Đổi tên nền tảng, thiết lập giới hạn dung lượng tải lên, bật/tắt chế độ bảo trì (**Maintenance Mode**).

---

## 4. Cấu trúc Thư mục Dự án

```text
ShareStudy/
├── .env                          # File biến môi trường cục bộ (được gitignore)
├── .env.example                  # File mẫu khai báo biến môi trường chuẩn
├── .gitignore                    # Cấu hình bỏ qua file trong Git
├── docker-compose.yml            # Khởi động dịch vụ PostgreSQL & MinIO cục bộ
├── DEPLOYMENT_GUIDE.md           # Hướng dẫn chi tiết từng bước triển khai Cloud
├── README.md                     # Tài liệu tổng quan dự án
│
├── client/                       # SOURCE CODE FRONTEND (React 19 + Vite)
│   ├── public/                   # Static assets (favicon, logos,...)
│   ├── src/
│   │   ├── assets/               # Hình ảnh, icons giao diện
│   │   ├── components/           # UI Components dùng chung (Header, Footer, Navbar, Card,...)
│   │   ├── config/               # Cấu hình Ant Design theme, màu sắc
│   │   ├── contexts/             # React Contexts (AuthContext, ThemeContext, SettingsContext)
│   │   ├── pages/                # Các trang màn hình chính:
│   │   │   ├── Home/             # Trang chủ, banner tìm kiếm
│   │   │   ├── Documents/        # Danh sách & Bộ lọc tài liệu
│   │   │   ├── DocumentDetail/   # Chi tiết tài liệu, PDF Viewer, Bình luận
│   │   │   ├── UploadDocument/   # Form tải tài liệu lên
│   │   │   ├── Admin/            # Các trang quản trị (Dashboard, Users, Documents, Categories, Settings)
│   │   │   ├── Login/ & Register/# Đăng nhập, Đăng ký tài khoản
│   │   │   └── Profile/          # Thông tin cá nhân, tài liệu đã tải
│   │   ├── routes/               # Quản lý định tuyến và phân quyền bảo vệ (AppRoutes)
│   │   ├── services/             # Axios API services gọi Backend
│   │   ├── utils/                # Hàm tiện ích (format date, validation, avatar,...)
│   │   ├── App.jsx / main.jsx    # Entry points của ứng dụng
│   │   └── index.css             # Tailwind CSS & Global styles
│   ├── package.json              # Danh sách thư viện Frontend
│   └── vite.config.ts            # Cấu hình Vite bundler
│
└── server/                       # SOURCE CODE BACKEND (.NET 8 Clean Architecture)
    ├── Dockerfile                # Dockerfile build và run API container cho Render
    ├── ShareStudy.sln            # Solution file của dự án .NET
    └── src/
        ├── ShareStudy.Domain/    # [Core] Entities, Enums, Domain Events
        │   ├── Entities/         # Document, User, School, Subject, Comment, Rating, AuditLog,...
        │   └── Enums/            # DocumentStatus, EducationTier, RoleType,...
        │
        ├── ShareStudy.Application/ # [Business Logic] DTOs, Service Interfaces, Services
        │   ├── DTOs/             # Request / Response Data Transfer Objects
        │   ├── Interfaces/       # IUnitOfWork, IRepository, IFileStorageService, IAuthService,...
        │   └── Services/         # AuthService, DocumentService, EducationService, AdminService,...
        │
        ├── ShareStudy.Infrastructure/ # [Implementation] DB Context, Repositories, S3/MinIO
        │   ├── Persistence/      # ApplicationDbContext, EF Configurations, DbInitializer (Seed)
        │   ├── Repositories/     # Repository<T>, UnitOfWork triển khai thực tế
        │   ├── Security/         # JwtTokenGenerator tạo và verify token
        │   └── Storage/          # MinioFileStorageService & R2FileStorageService
        │
        └── ShareStudy.Api/       # [Presentation] API Controllers, Middlewares, DI
            ├── Controllers/      # Auth, Documents, Education, Comments, Ratings, Admin/*
            ├── Middlewares/      # ExceptionHandling, MaintenanceMode, AuditLogging
            ├── Services/         # CurrentUserService
            ├── appsettings.json  # Cấu hình rỗng (secrets chuyển hết vào .env)
            └── Program.cs        # Entry point: Nạp .env, cấu hình DI, Swagger, CORS, Middleware
```

---

## 5. Cấu hình Biến môi trường (.env)

Hệ thống quản lý biến môi trường tập trung thông qua file `.env` tại thư mục gốc. File này tự động cấp tham số cho cả **Docker Compose** và **Backend ASP.NET Core 8**.

### Khởi tạo file `.env`:
```bash
# Trên Windows PowerShell:
Copy-Item .env.example .env

# Hoặc trên Linux / macOS:
cp .env.example .env
```

### Bảng tra cứu Biến môi trường:

| Nhóm | Tên Biến | Giá trị Mặc định (Local) | Mô tả Chi tiết |
|---|---|---|---|
| **PostgreSQL** | `POSTGRES_DB` | `sharestudy` | Tên Database PostgreSQL |
| | `POSTGRES_USER` | `postgres` | Tài khoản quản trị PostgreSQL |
| | `POSTGRES_PASSWORD` | `26092004` | Mật khẩu truy cập PostgreSQL |
| | `POSTGRES_PORT` | `5432` | Cổng kết nối PostgreSQL ngoài máy host |
| | `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=sharestudy;Username=postgres;Password=26092004` | Chuỗi kết nối Npgsql cho Entity Framework Core |
| **MinIO Storage** | `MINIO_ROOT_USER` / `MINIO_ACCESS_KEY` | `minioadmin` | Tài khoản quản trị root / Access Key |
| | `MINIO_ROOT_PASSWORD` / `MINIO_SECRET_KEY` | `minioadmin` | Mật khẩu root / Secret Key |
| | `MINIO_PORT` | `9000` | Cổng API Object Storage (S3 API) |
| | `MINIO_CONSOLE_PORT` | `9001` | Cổng Web UI MinIO Console |
| | `MINIO_BUCKET_NAME` | `documents` | Tên bucket mặc định chứa file |
| | `MINIO_USE_SSL` | `false` | Bật/tắt SSL (Local để `false`) |
| **Bảo mật JWT** | `JWT_SECRET` | *(chuỗi >= 32 bytes)* | Khóa bí mật dùng để ký và xác thực token JWT |
| | `JWT_ISSUER` | `ShareStudyApi` | Đơn vị phát hành token |
| | `JWT_AUDIENCE` | `ShareStudyClient` | Đối tượng sử dụng token |
| | `JWT_EXPIRY_MINUTES` | `30` | Thời gian sống của Access Token (phút) |
| **Frontend** | `VITE_API_URL` | `http://localhost:5000/api` | Đường dẫn API Backend cho Frontend gọi request |

---

## 6. Hướng dẫn Khởi chạy Môi trường Local

### Yêu cầu tiên quyết:
- **Git** installed
- **Node.js**: v18.0.0+ và **npm** v9.0.0+
- **.NET 8.0 SDK**: v8.0 trở lên ([Tải tại dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Docker Desktop**: Đang chạy trên máy

---

### Bước 1: Clone Repository & Chuẩn bị File Cấu hình
```bash
git clone https://github.com/<your-username>/ShareStudy.git
cd ShareStudy

# Sao chép file cấu hình mẫu thành file .env thực tế
Copy-Item .env.example .env    # PowerShell
# hoặc: cp .env.example .env   # Bash
```

### Bước 2: Khởi động Database & Object Storage qua Docker
Tại thư mục gốc `ShareStudy/`, chạy:
```bash
docker compose up -d postgres minio
```
Kiểm tra container đang chạy:
```bash
docker compose ps
```
* **PostgreSQL Database**: `localhost:5432`
* **MinIO Storage API**: `http://localhost:9000`
* **MinIO Web Console**: [http://localhost:9001](http://localhost:9001) *(User: `minioadmin` | Pass: `minioadmin`)*

### Bước 3: Khởi chạy Backend Web API (.NET 8)
Mở cửa sổ Terminal thứ nhất:
```bash
cd server/src/ShareStudy.Api
dotnet run
```
* **API Endpoint**: `http://localhost:5000`
* **Tài liệu Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

> **Lưu ý**: Khi chạy lần đầu, ứng dụng sẽ tự động áp dụng EF Core Migrations và nạp sẵn dữ liệu mẫu (Seed Data) gồm các trường học, môn học, tài khoản test và danh mục mẫu.

### Bước 4: Khởi chạy Frontend Client (React Vite)
Mở cửa sổ Terminal thứ hai:
```bash
cd client
npm install
npm run dev
```
* **Truy cập Giao diện Người dùng**: [http://localhost:5173](http://localhost:5173)

---

## 7. Tài khoản Đăng nhập Mặc định

Hệ thống đã nạp sẵn 3 tài khoản tương ứng với 3 cấp độ phân quyền để thuận tiện kiểm thử:

| Vai trò (Role) | Username / Email | Mật khẩu | Quyền hạn và Phạm vi hoạt động |
|---|---|---|---|
| **System Admin** | `admin@sharestudy.vn`<br>`admin` | `Admin@123` | Toàn quyền quản trị hệ thống, bảng điều khiển thống kê, quản lý người dùng, quản lý danh mục giáo dục, xem Audit Logs, cấu hình bảo trì. |
| **Moderator** | `moderator@sharestudy.vn`<br>`mcv` | `Mod@123` | Phê duyệt/từ chối/ẩn tài liệu tải lên, tiếp nhận và giải quyết các báo cáo vi phạm nội dung. |
| **Member** | `student_140626` | `Student@123` | Tìm kiếm học liệu, xem trước PDF, tải file, đánh giá sao, bình luận và quản lý tài liệu cá nhân. |

---

## 8. Tổng quan API Endpoints

Hệ thống cung cấp đầy đủ tài liệu tương tác trực tiếp qua **Swagger UI** tại đường dẫn: `http://localhost:5000/swagger`.

Dưới đây là tóm tắt các cụm endpoint chính:

| Nhóm API | Method | Đường dẫn Endpoint | Mô tả chức năng | Yêu cầu quyền |
|---|---|---|---|---|
| **Auth** | `POST` | `/api/auth/login` | Đăng nhập hệ thống, nhận JWT Token | Public |
| | `POST` | `/api/auth/register` | Đăng ký tài khoản thành viên mới | Public |
| | `GET` | `/api/auth/me` | Lấy thông tin tài khoản hiện tại | Đã đăng nhập |
| **Documents** | `GET` | `/api/documents` | Tìm kiếm, phân trang và lọc tài liệu | Public |
| | `GET` | `/api/documents/{id}` | Lấy chi tiết tài liệu & tăng lượt xem | Public |
| | `POST` | `/api/documents` | Đăng tải tài liệu mới lên hệ thống | Member / Mod / Admin |
| | `GET` | `/api/documents/{id}/download` | Lấy Presigned URL tải file an toàn | Đã đăng nhập |
| **Education** | `GET` | `/api/education/levels` | Danh sách Cấp học (THCS, THPT, Đại học) | Public |
| | `GET` | `/api/education/schools` | Lọc danh sách Trường học theo cấp học | Public |
| | `GET` | `/api/education/subjects` | Lấy danh sách Môn học | Public |
| **Interactions**| `GET` | `/api/comments/document/{docId}` | Lấy cây bình luận đa tầng của tài liệu | Public |
| | `POST` | `/api/comments` | Thêm bình luận hoặc phản hồi | Đã đăng nhập |
| | `POST` | `/api/ratings` | Đánh giá số sao (1 - 5) cho tài liệu | Đã đăng nhập |
| | `POST` | `/api/favorites/{docId}` | Thêm/Gỡ tài liệu khỏi mục yêu thích | Đã đăng nhập |
| | `POST` | `/api/reports` | Gửi báo cáo vi phạm tài liệu | Đã đăng nhập |
| **Admin** | `GET` | `/api/admin/dashboard/stats` | Thống kê số liệu biểu đồ tổng quan | Admin |
| | `GET` | `/api/admin/users` | Quản lý danh sách thành viên | Admin |
| | `PUT` | `/api/admin/documents/{id}/status` | Duyệt / Từ chối / Ẩn tài liệu | Mod / Admin |
| | `GET` | `/api/admin/audit-logs` | Lịch sử nhật ký thao tác hệ thống | Admin |
| | `PUT` | `/api/admin/settings` | Cập nhật cấu hình hệ thống & Bảo trì | Admin |

---

## 9. Triển khai Production Cloud

Dự án đã được tối ưu sẵn sàng triển khai trên các nền tảng Cloud hiện đại:

1. **Database** $\rightarrow$ [Neon.tech](https://neon.tech): PostgreSQL Serverless với chuỗi kết nối SSL.
2. **File Storage** $\rightarrow$ [Cloudflare R2](https://cloudflare.com): Lưu trữ chuẩn S3 không tính phí băng thông tải về (Zero Egress Fee).
3. **Backend API** $\rightarrow$ [Render.com](https://render.com): Chạy Web Service qua Dockerfile (`server/Dockerfile`).
4. **Frontend Client** $\rightarrow$ [Vercel.com](https://vercel.com): Triển khai SPA tự động từ nhánh GitHub, hỗ trợ rewrite URL qua `vercel.json`.

> **Xem hướng dẫn chi tiết từng bước nhấp chuột**: Vui lòng tham khảo file [DEPLOYMENT_GUIDE.md](file:///d:/D%20COPY/MAICHIVINH/DevVinh/ShareStudy/DEPLOYMENT_GUIDE.md).

---

## 10. Bảo mật & An toàn Thông tin

* **Authentication & Authorization**: Sử dụng JWT Token chuẩn HMAC-SHA256 với khóa bí mật tối thiểu 256-bit, phân tách chính sách nghiêm ngặt (`AdminOnly`, `ModeratorOrAdmin`).
* **Phòng chống SQL Injection**: Mọi truy vấn CSDL đều thực hiện qua Entity Framework Core với Parameterized Queries.
* **CORS Configuration**: Kiểm soát chặt chẽ danh sách domain được phép gọi API (Hỗ trợ Localhost và Vercel Domain).
* **Bảo vệ File Storage**: Không public trực tiếp đường dẫn file lưu trữ nội bộ; tải file qua cơ chế **Presigned URL** có thời hạn hiệu lực.
* **Chế độ Bảo trì (Maintenance Mode)**: Cho phép quản trị viên tạm khóa tương tác toàn hệ thống khi tiến hành nâng cấp hoặc bảo trì mà không làm gián đoạn hạ tầng.
* **Audit Logging**: Tự động ghi nhận thông tin thời gian, tài khoản, tác vụ và địa chỉ IP thực hiện đối với mọi hành vi can thiệp dữ liệu.

---

## 11. Hướng dẫn Đóng góp (Contributing)

Chúng tôi luôn hoan nghênh mọi sự đóng góp từ cộng đồng:
1. **Fork** repository này về tài khoản GitHub của bạn.
2. Tạo nhánh tính năng mới: `git checkout -b feature/tinh-nang-moi`.
3. Commit thay đổi theo chuẩn Conventional Commits: `git commit -m "feat: bổ sung tính năng X"`.
4. Push lên nhánh của bạn: `git push origin feature/tinh-nang-moi`.
5. Tạo một **Pull Request** chi tiết mô tả những thay đổi bạn đã thực hiện.

---

<div align="center">

**Developed by ShareStudy Team © 2026**  
*Mọi thắc mắc hoặc yêu cầu hỗ trợ, vui lòng mở Issue trên GitHub Repository.*

</div>
