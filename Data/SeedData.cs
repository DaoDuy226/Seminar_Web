using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

/// <summary>
/// Khởi tạo dữ liệu mẫu thông qua EF Core ModelBuilder.HasData:
/// 5 thể loại, 20 sách, 10 độc giả, 7 phiếu mượn (3 quá hạn, 2 đang mượn, 2 đã trả) và 12 chi tiết phiếu mượn.
/// </summary>
public static class SeedData
{
    public static void SeedLibraryData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Công nghệ thông tin" },
            new Category { Id = 2, Name = "Văn học" },
            new Category { Id = 3, Name = "Kinh tế & Quản trị" },
            new Category { Id = 4, Name = "Khoa học & Đời sống" },
            new Category { Id = 5, Name = "Kỹ năng sống" }
        );

        modelBuilder.Entity<Book>().HasData(
            // 1. Công nghệ thông tin
            new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", CategoryId = 1, Quantity = 5, Available = 4, PublishYear = 2008 },
            new Book { Id = 2, Title = "Pro ASP.NET Core MVC", Author = "Adam Freeman", CategoryId = 1, Quantity = 6, Available = 5, PublishYear = 2024 },
            new Book { Id = 3, Title = "Design Patterns", Author = "Erich Gamma", CategoryId = 1, Quantity = 4, Available = 3, PublishYear = 1994 },
            new Book { Id = 4, Title = "Lập Trình C# Căn Bản", Author = "Phạm Văn Ất", CategoryId = 1, Quantity = 8, Available = 8, PublishYear = 2023 },

            // 2. Văn học
            new Book { Id = 5, Title = "Số Đỏ", Author = "Vũ Trọng Phụng", CategoryId = 2, Quantity = 5, Available = 5, PublishYear = 1936 },
            new Book { Id = 6, Title = "Dế Mèn Phiêu Lưu Ký", Author = "Tô Hoài", CategoryId = 2, Quantity = 7, Available = 7, PublishYear = 1941 },
            new Book { Id = 7, Title = "Mắt Biếc", Author = "Nguyễn Nhật Ánh", CategoryId = 2, Quantity = 6, Available = 4, PublishYear = 1990 },
            new Book { Id = 8, Title = "Nhà Giả Kim", Author = "Paulo Coelho", CategoryId = 2, Quantity = 8, Available = 8, PublishYear = 1988 },

            // 3. Kinh tế & Quản trị
            new Book { Id = 9, Title = "Cha Giàu Cha Nghèo", Author = "Robert T. Kiyosaki", CategoryId = 3, Quantity = 6, Available = 6, PublishYear = 1997 },
            new Book { Id = 10, Title = "Từ Tốt Đến Vĩ Đại", Author = "Jim Collins", CategoryId = 3, Quantity = 4, Available = 4, PublishYear = 2001 },
            new Book { Id = 11, Title = "Khởi Nghiệp Tinh Gọn", Author = "Eric Ries", CategoryId = 3, Quantity = 5, Available = 5, PublishYear = 2011 },
            new Book { Id = 12, Title = "Tâm Lý Học Về Tiền", Author = "Morgan Housel", CategoryId = 3, Quantity = 7, Available = 6, PublishYear = 2020 },

            // 4. Khoa học & Đời sống
            new Book { Id = 13, Title = "Lược Sử Thời Gian", Author = "Stephen Hawking", CategoryId = 4, Quantity = 5, Available = 5, PublishYear = 1988 },
            new Book { Id = 14, Title = "Sapiens: Lược Sử Loài Người", Author = "Yuval Noah Harari", CategoryId = 4, Quantity = 6, Available = 5, PublishYear = 2011 },
            new Book { Id = 15, Title = "Vũ Trụ", Author = "Carl Sagan", CategoryId = 4, Quantity = 4, Available = 4, PublishYear = 1980 },
            new Book { Id = 16, Title = "Gen: Lịch Sử Và Tương Lai", Author = "Siddhartha Mukherjee", CategoryId = 4, Quantity = 3, Available = 3, PublishYear = 2016 },

            // 5. Kỹ năng sống
            new Book { Id = 17, Title = "Đắc Nhân Tâm", Author = "Dale Carnegie", CategoryId = 5, Quantity = 10, Available = 8, PublishYear = 1936 },
            new Book { Id = 18, Title = "Thói Quen Nguyên Tử", Author = "James Clear", CategoryId = 5, Quantity = 8, Available = 7, PublishYear = 2018 },
            new Book { Id = 19, Title = "Tuổi Trẻ Đáng Giá Bao Nhiêu", Author = "Rosie Nguyễn", CategoryId = 5, Quantity = 6, Available = 6, PublishYear = 2016 },
            new Book { Id = 20, Title = "Cà Phê Cùng Tony", Author = "Tony Buổi Sáng", CategoryId = 5, Quantity = 7, Available = 7, PublishYear = 2015 }
        );

        modelBuilder.Entity<Reader>().HasData(
            new Reader { Id = 1, FullName = "Nguyễn Minh Anh", Phone = "0901234001", Email = "minhanh01@gmail.com" },
            new Reader { Id = 2, FullName = "Trần Hoàng Bảo", Phone = "0901234002", Email = "hoangbao02@gmail.com" },
            new Reader { Id = 3, FullName = "Lê Thảo Chi", Phone = "0901234003", Email = "thaochi03@gmail.com" },
            new Reader { Id = 4, FullName = "Phạm Quốc Dũng", Phone = "0901234004", Email = "quocdung04@gmail.com" },
            new Reader { Id = 5, FullName = "Võ Ngọc Hà", Phone = "0901234005", Email = "ngocha05@gmail.com" },
            new Reader { Id = 6, FullName = "Đỗ Gia Huy", Phone = "0901234006", Email = "giahuy06@gmail.com" },
            new Reader { Id = 7, FullName = "Bùi Khánh Linh", Phone = "0901234007", Email = "khanhlinh07@gmail.com" },
            new Reader { Id = 8, FullName = "Hoàng Đức Mạnh", Phone = "0901234008", Email = "ducmanh08@gmail.com" },
            new Reader { Id = 9, FullName = "Ngô Thanh Ngân", Phone = "0901234009", Email = "thanhngan09@gmail.com" },
            new Reader { Id = 10, FullName = "Dương Tuấn Kiệt", Phone = "0901234010", Email = "tuankiet10@gmail.com" }
        );

        modelBuilder.Entity<BorrowRecord>().HasData(
            // 3 phiếu quá hạn (DueDate trước ngày 13/11/2026 và ReturnDate == null)
            new BorrowRecord
            {
                Id = 1,
                ReaderId = 1,
                BorrowDate = new DateTime(2026, 10, 15),
                DueDate = new DateTime(2026, 10, 29),
                ReturnDate = null,
                Status = BorrowStatuses.Overdue
            },
            new BorrowRecord
            {
                Id = 2,
                ReaderId = 2,
                BorrowDate = new DateTime(2026, 10, 20),
                DueDate = new DateTime(2026, 11, 3),
                ReturnDate = null,
                Status = BorrowStatuses.Overdue
            },
            new BorrowRecord
            {
                Id = 3,
                ReaderId = 3,
                BorrowDate = new DateTime(2026, 10, 25),
                DueDate = new DateTime(2026, 11, 8),
                ReturnDate = null,
                Status = BorrowStatuses.Overdue
            },
            // 2 phiếu đang mượn còn hạn (14 ngày, DueDate sau ngày 25/11/2026 để giữ trạng thái Đang mượn suốt đợt demo)
            new BorrowRecord
            {
                Id = 4,
                ReaderId = 4,
                BorrowDate = new DateTime(2026, 11, 12),
                DueDate = new DateTime(2026, 11, 26),
                ReturnDate = null,
                Status = BorrowStatuses.Borrowing
            },
            new BorrowRecord
            {
                Id = 5,
                ReaderId = 5,
                BorrowDate = new DateTime(2026, 11, 13),
                DueDate = new DateTime(2026, 11, 27),
                ReturnDate = null,
                Status = BorrowStatuses.Borrowing
            },
            // 2 phiếu đã trả
            new BorrowRecord
            {
                Id = 6,
                ReaderId = 6,
                BorrowDate = new DateTime(2026, 10, 1),
                DueDate = new DateTime(2026, 10, 15),
                ReturnDate = new DateTime(2026, 10, 12),
                Status = BorrowStatuses.Returned
            },
            new BorrowRecord
            {
                Id = 7,
                ReaderId = 7,
                BorrowDate = new DateTime(2026, 10, 18),
                DueDate = new DateTime(2026, 11, 1),
                ReturnDate = new DateTime(2026, 10, 31),
                Status = BorrowStatuses.Returned
            }
        );

        modelBuilder.Entity<BorrowDetail>().HasData(
            // Chi tiết cho các phiếu quá hạn (Id 1..3)
            new BorrowDetail { Id = 1, BorrowRecordId = 1, BookId = 1, Quantity = 1 },
            new BorrowDetail { Id = 2, BorrowRecordId = 1, BookId = 2, Quantity = 1 },
            new BorrowDetail { Id = 3, BorrowRecordId = 2, BookId = 7, Quantity = 2 },
            new BorrowDetail { Id = 4, BorrowRecordId = 3, BookId = 14, Quantity = 1 },
            new BorrowDetail { Id = 5, BorrowRecordId = 3, BookId = 18, Quantity = 1 },

            // Chi tiết cho các phiếu đang mượn (Id 4..5)
            new BorrowDetail { Id = 6, BorrowRecordId = 4, BookId = 17, Quantity = 2 },
            new BorrowDetail { Id = 7, BorrowRecordId = 5, BookId = 3, Quantity = 1 },
            new BorrowDetail { Id = 8, BorrowRecordId = 5, BookId = 12, Quantity = 1 },

            // Chi tiết cho các phiếu đã trả (Id 6..7)
            new BorrowDetail { Id = 9, BorrowRecordId = 6, BookId = 8, Quantity = 1 },
            new BorrowDetail { Id = 10, BorrowRecordId = 6, BookId = 17, Quantity = 1 },
            new BorrowDetail { Id = 11, BorrowRecordId = 7, BookId = 5, Quantity = 1 },
            new BorrowDetail { Id = 12, BorrowRecordId = 7, BookId = 9, Quantity = 1 }
        );
    }
}
