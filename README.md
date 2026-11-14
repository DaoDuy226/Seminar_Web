# Demo Quản Lý Thư Viện — ASP.NET Core MVC & EF Core

Khung project nền tảng (Task **#8**) cho bài seminar **SE347 · ASP.NET Core MVC** 

---

## 1. Công nghệ sử dụng

- **Framework:** ASP.NET Core MVC (`.NET 9.0`)
- **ORM:** Entity Framework Core `9.0.0` (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`)
- **CSDL:** Microsoft SQL Server (SQL Server Express / LocalDB trên Windows, hoặc Docker SQL Server 2022 trên macOS)
- **Giao diện nền:** Bootstrap 5 + jQuery Validation (Client-side & Server-side Data Annotations)

---

## 2. Cấu trúc project & Mô hình dữ liệu

```text
demoaspnetcoremvc/
├── Controllers/
│   └── HomeController.cs                 # Dashboard tổng quan & các trang xem nhanh dữ liệu mẫu
├── Data/
│   ├── LibraryDbContext.cs               # DbContext cấu hình quan hệ, Check Constraint, Index & DeleteBehavior
│   └── SeedData.cs                       # Dữ liệu mẫu (5 thể loại, 20 sách, 10 độc giả, 7 phiếu mượn)
├── docs/
│   └── P3-Model-va-EF-Core.md            # Tài liệu lý thuyết P3 kèm ví dụ trích từ demo
├── Migrations/                           # EF Core Migrations (InitialCreate)
├── Models/
│   ├── BorrowStatuses.cs                 # Hằng số trạng thái ("Đang mượn", "Đã trả", "Quá hạn")
│   ├── Category.cs                       # Thể loại sách
│   ├── Book.cs                           # Sách + Data Annotations + IValidatableObject
│   ├── Reader.cs                         # Độc giả
│   ├── BorrowRecord.cs                   # Phiếu mượn + kiểm tra ngày hợp lệ
│   ├── BorrowDetail.cs                   # Chi tiết phiếu mượn
│   └── ViewModels/
│       ├── DashboardViewModel.cs         # ViewModel dùng chung cho Dashboard (#13, #14)
│       └── LibraryPagesViewModels.cs     # ViewModel cho trang Kho sách & Độc giả
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml                  # Dashboard tổng quan
│   │   ├── Books.cshtml                  # Danh mục & lọc kho sách
│   │   ├── Categories.cshtml             # Danh sách thể loại sách
│   │   ├── Readers.cshtml                # Danh sách độc giả
│   │   ├── Borrows.cshtml                # Danh sách phiếu mượn / trả
│   │   ├── Overdue.cshtml                # Danh sách phiếu quá hạn
│   │   └── Statistics.cshtml             # Khung thống kê (#14)
│   └── Shared/_Layout.cshtml             # Layout Bootstrap 5 tiếng Việt
├── wwwroot/
│   ├── css/site.css                      # Stylesheet giao diện & hiệu ứng hover
│   └── images/                           # Logo SVG, banner & ảnh minh họa thư viện
├── appsettings.json                      # Chuỗi kết nối mặc định (.\SQLEXPRESS)
├── appsettings.Development.example.json  # Mẫu override cấu hình cá nhân
├── docker-compose.yml                    # Chạy SQL Server 2022 bằng Docker (macOS / Linux / Windows)
└── Program.cs                            # Đăng ký DbContext, tự động Migrate() & MVC pipeline
```

### Tóm tắt 5 Entity, Ràng buộc SQL & Quy tắc Validation

| Model | Thuộc tính chính | Validation & Ràng buộc SQL Server |
|---|---|---|
| `Category` | `Id`, `Name` | `[Required]`, `[StringLength(100, MinimumLength = 2)]`, Unique Index `IX_Categories_Name` |
| `Book` | `Id`, `Title`, `Author`, `CategoryId`, `Quantity`, `Available`, `PublishYear` | `[Required]`, `[StringLength]`, `[Range]`, `Available <= Quantity`, Check Constraints `CK_Books_Quantity`, `CK_Books_Available`, `CK_Books_PublishYear`, `DeleteBehavior.Restrict` |
| `Reader` | `Id`, `FullName`, `Phone`, `Email` | `[Required]`, `[StringLength]`, `[RegularExpression(@"^0\d{8,10}$")]`, `[EmailAddress]`, Unique Index `IX_Readers_Email`, `IX_Readers_Phone` |
| `BorrowRecord` | `Id`, `ReaderId`, `BorrowDate`, `DueDate`, `ReturnDate?`, `Status` | `[Required]`, `[DataType(DataType.Date)]`, Check Constraints `CK_BorrowRecords_DueDate`, `CK_BorrowRecords_ReturnDate`, `DeleteBehavior.Restrict` |
| `BorrowDetail` | `Id`, `BorrowRecordId`, `BookId`, `Quantity` | `[Required]`, `[Range(1, 50)]`, Check Constraint `CK_BorrowDetails_Quantity`, Unique Composite Index `IX_BorrowDetails_BorrowRecordId_BookId`, `Cascade` theo `BorrowRecord` |

### Dữ liệu mẫu (Seed Data)

Khi chạy `Update-Database`, hệ thống tự động tạo sẵn:
- **5 thể loại:** Công nghệ thông tin, Văn học, Kinh tế & Quản trị, Khoa học & Đời sống, Kỹ năng sống.
- **20 đầu sách** (tổng **120 cuốn**, còn trong kho **110 cuốn**).
- **10 độc giả** (mã 1–10).
- **7 phiếu mượn** (12 chi tiết mượn), trong đó có **3 phiếu quá hạn**, **2 phiếu đang mượn**, **2 phiếu đã trả**. Số lượng `Available` của từng sách khớp chính xác với các phiếu chưa trả.

---

## 3. Yêu cầu cài đặt trước khi chạy

1. **.NET 9.0 SDK** — kiểm tra bằng lệnh:
   ```bash
   dotnet --version
   ```
2. **EF Core CLI** *(tùy chọn — chỉ cần khi tạo Migration mới hoặc reset DB qua terminal; lúc chạy app đã tự động `Migrate()`)*:
   ```bash
   dotnet tool install --global dotnet-ef --version 9.0.0
   ```
3. **SQL Server** theo hệ điều hành bên dưới.

---

## 4. Hướng dẫn clone và chạy trên Windows

### Bước 1: Clone repository

```bash
git clone https://github.com/DaoDuy226/Seminar_Web.git
cd Seminar_Web
```

### Bước 2: Kiểm tra chuỗi kết nối SQL Server

File `appsettings.json` đang cấu hình mặc định cho **SQL Server Express**:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=LibraryManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Nếu máy bạn dùng **Visual Studio LocalDB** (không dùng `SQLEXPRESS`), **không cần sửa `appsettings.json`** (tránh xung đột Git). Hãy tạo file `appsettings.Development.json` (đã được `.gitignore` bỏ qua) với nội dung:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Bước 3: Chạy ứng dụng (tự động tạo DB & nạp dữ liệu mẫu)

`Program.cs` đã cấu hình sẵn `dbContext.Database.Migrate()`, nên chỉ cần chạy ứng dụng là EF Core tự động tạo database `LibraryManagementDb`, áp dụng Migration và nạp toàn bộ dữ liệu mẫu:

```bash
dotnet run
```
(Hoặc mở `LibraryManagement.sln` trong Visual Studio 2022 và nhấn `F5`).  
Mở trình duyệt tại địa chỉ hiển thị trong terminal (mặc định `http://localhost:5057` hoặc `https://localhost:7275`) để xem Dashboard cùng toàn bộ dữ liệu mẫu.

*(Tùy chọn — nếu muốn chạy Migration thủ công trước khi chạy web: dùng `dotnet ef database update` hoặc `Update-Database` trong Package Manager Console).*

---

## 5. Hướng dẫn chạy SQL Server bằng Docker cho macOS (và Linux)

### Bước 1: Cài Docker Desktop

Cài [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop/) và khởi động Docker.

> **Lưu ý cho máy Mac chip Apple Silicon (M1 / M2 / M3 / M4):**  
> SQL Server 2022 dùng kiến trúc `linux/amd64`. Hãy mở **Docker Desktop -> Settings -> General** và bật tùy chọn **"Use Rosetta for x86_64/amd64 emulation on Apple Silicon"** để container chạy ổn định và nhanh hơn.

### Bước 2: Khởi chạy SQL Server 2022 container

Chạy bằng `docker compose` (khuyến nghị, đã cấu hình sẵn trong `docker-compose.yml`):

```bash
docker compose up -d
```

Hoặc chạy trực tiếp bằng lệnh `docker run`:

```bash
docker run -d --name library-sqlserver \
  --platform linux/amd64 \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Library@2026" \
  -e "MSSQL_PID=Express" \
  -p 1433:1433 \
  -v library_sqlserver_data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

Chờ khoảng 10–15 giây cho SQL Server khởi động xong, kiểm tra trạng thái:

```bash
docker ps
```

### Bước 3: Tạo file `appsettings.Development.json` cho Mac

Tạo file `appsettings.Development.json` ở thư mục gốc của project (file này đã nằm trong `.gitignore`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LibraryManagementDb;User Id=sa;Password=Library@2026;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Bước 4: Chạy ứng dụng & mở web

```bash
dotnet run
```
*(Ứng dụng tự động tạo database và nạp dữ liệu mẫu ngay lần chạy đầu tiên; nếu muốn chạy tay trước có thể dùng `dotnet ef database update`).*

Truy cập `http://localhost:5057` trên trình duyệt.

*(Khi cần dừng SQL Server Docker: `docker compose down`; nếu muốn xóa sạch volume dữ liệu để tạo lại từ đầu: `docker compose down -v`.)*

---

## 6. Đưa database về dữ liệu mẫu trước giờ demo

Trong quá trình test hoặc tập demo có thể phát sinh dữ liệu thử nghiệm. Để khôi phục nguyên trạng dữ liệu mẫu ban đầu:

- **Bằng Terminal:**
  ```bash
  dotnet ef database drop --force
  dotnet ef database update
  ```
- **Bằng Package Manager Console (Visual Studio):**
  ```powershell
  Drop-Database -Confirm:$false
  Update-Database
  ```
