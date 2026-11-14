using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Available = table.Column<int>(type: "int", nullable: false),
                    PublishYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.CheckConstraint("CK_Books_Available", "[Available] >= 0 AND [Available] <= [Quantity]");
                    table.CheckConstraint("CK_Books_PublishYear", "[PublishYear] >= 1900 AND [PublishYear] <= 2026");
                    table.CheckConstraint("CK_Books_Quantity", "[Quantity] >= 1 AND [Quantity] <= 1000");
                    table.ForeignKey(
                        name: "FK_Books_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReaderId = table.Column<int>(type: "int", nullable: false),
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.Id);
                    table.CheckConstraint("CK_BorrowRecords_DueDate", "[DueDate] >= [BorrowDate]");
                    table.CheckConstraint("CK_BorrowRecords_ReturnDate", "[ReturnDate] IS NULL OR [ReturnDate] >= [BorrowDate]");
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Readers_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "Readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BorrowDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorrowRecordId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowDetails", x => x.Id);
                    table.CheckConstraint("CK_BorrowDetails_Quantity", "[Quantity] >= 1 AND [Quantity] <= 50");
                    table.ForeignKey(
                        name: "FK_BorrowDetails_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowDetails_BorrowRecords_BorrowRecordId",
                        column: x => x.BorrowRecordId,
                        principalTable: "BorrowRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Công nghệ thông tin" },
                    { 2, "Văn học" },
                    { 3, "Kinh tế & Quản trị" },
                    { 4, "Khoa học & Đời sống" },
                    { 5, "Kỹ năng sống" }
                });

            migrationBuilder.InsertData(
                table: "Readers",
                columns: new[] { "Id", "Email", "FullName", "Phone" },
                values: new object[,]
                {
                    { 1, "minhanh01@gmail.com", "Nguyễn Minh Anh", "0901234001" },
                    { 2, "hoangbao02@gmail.com", "Trần Hoàng Bảo", "0901234002" },
                    { 3, "thaochi03@gmail.com", "Lê Thảo Chi", "0901234003" },
                    { 4, "quocdung04@gmail.com", "Phạm Quốc Dũng", "0901234004" },
                    { 5, "ngocha05@gmail.com", "Võ Ngọc Hà", "0901234005" },
                    { 6, "giahuy06@gmail.com", "Đỗ Gia Huy", "0901234006" },
                    { 7, "khanhlinh07@gmail.com", "Bùi Khánh Linh", "0901234007" },
                    { 8, "ducmanh08@gmail.com", "Hoàng Đức Mạnh", "0901234008" },
                    { 9, "thanhngan09@gmail.com", "Ngô Thanh Ngân", "0901234009" },
                    { 10, "tuankiet10@gmail.com", "Dương Tuấn Kiệt", "0901234010" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Available", "CategoryId", "PublishYear", "Quantity", "Title" },
                values: new object[,]
                {
                    { 1, "Robert C. Martin", 4, 1, 2008, 5, "Clean Code" },
                    { 2, "Adam Freeman", 5, 1, 2024, 6, "Pro ASP.NET Core MVC" },
                    { 3, "Erich Gamma", 3, 1, 1994, 4, "Design Patterns" },
                    { 4, "Phạm Văn Ất", 8, 1, 2023, 8, "Lập Trình C# Căn Bản" },
                    { 5, "Vũ Trọng Phụng", 5, 2, 1936, 5, "Số Đỏ" },
                    { 6, "Tô Hoài", 7, 2, 1941, 7, "Dế Mèn Phiêu Lưu Ký" },
                    { 7, "Nguyễn Nhật Ánh", 4, 2, 1990, 6, "Mắt Biếc" },
                    { 8, "Paulo Coelho", 8, 2, 1988, 8, "Nhà Giả Kim" },
                    { 9, "Robert T. Kiyosaki", 6, 3, 1997, 6, "Cha Giàu Cha Nghèo" },
                    { 10, "Jim Collins", 4, 3, 2001, 4, "Từ Tốt Đến Vĩ Đại" },
                    { 11, "Eric Ries", 5, 3, 2011, 5, "Khởi Nghiệp Tinh Gọn" },
                    { 12, "Morgan Housel", 6, 3, 2020, 7, "Tâm Lý Học Về Tiền" },
                    { 13, "Stephen Hawking", 5, 4, 1988, 5, "Lược Sử Thời Gian" },
                    { 14, "Yuval Noah Harari", 5, 4, 2011, 6, "Sapiens: Lược Sử Loài Người" },
                    { 15, "Carl Sagan", 4, 4, 1980, 4, "Vũ Trụ" },
                    { 16, "Siddhartha Mukherjee", 3, 4, 2016, 3, "Gen: Lịch Sử Và Tương Lai" },
                    { 17, "Dale Carnegie", 8, 5, 1936, 10, "Đắc Nhân Tâm" },
                    { 18, "James Clear", 7, 5, 2018, 8, "Thói Quen Nguyên Tử" },
                    { 19, "Rosie Nguyễn", 6, 5, 2016, 6, "Tuổi Trẻ Đáng Giá Bao Nhiêu" },
                    { 20, "Tony Buổi Sáng", 7, 5, 2015, 7, "Cà Phê Cùng Tony" }
                });

            migrationBuilder.InsertData(
                table: "BorrowRecords",
                columns: new[] { "Id", "BorrowDate", "DueDate", "ReaderId", "ReturnDate", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, "Quá hạn" },
                    { 2, new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null, "Quá hạn" },
                    { 3, new DateTime(2026, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, null, "Quá hạn" },
                    { 4, new DateTime(2026, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, null, "Đang mượn" },
                    { 5, new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, null, "Đang mượn" },
                    { 6, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã trả" },
                    { 7, new DateTime(2026, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã trả" }
                });

            migrationBuilder.InsertData(
                table: "BorrowDetails",
                columns: new[] { "Id", "BookId", "BorrowRecordId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1, 1 },
                    { 2, 2, 1, 1 },
                    { 3, 7, 2, 2 },
                    { 4, 14, 3, 1 },
                    { 5, 18, 3, 1 },
                    { 6, 17, 4, 2 },
                    { 7, 3, 5, 1 },
                    { 8, 12, 5, 1 },
                    { 9, 8, 6, 1 },
                    { 10, 17, 6, 1 },
                    { 11, 5, 7, 1 },
                    { 12, 9, 7, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowDetails_BookId",
                table: "BorrowDetails",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowDetails_BorrowRecordId_BookId",
                table: "BorrowDetails",
                columns: new[] { "BorrowRecordId", "BookId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_ReaderId",
                table: "BorrowRecords",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readers_Email",
                table: "Readers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readers_Phone",
                table: "Readers",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowDetails");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Readers");
        }
    }
}
