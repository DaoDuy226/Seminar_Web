using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

/// <summary>
/// DbContext chính của ứng dụng Quản lý Thư viện (EF Core Code First).
/// </summary>
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
    public DbSet<BorrowDetail> BorrowDetails => Set<BorrowDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Books_Quantity", "[Quantity] >= 1 AND [Quantity] <= 1000");
                t.HasCheckConstraint("CK_Books_Available", "[Available] >= 0 AND [Available] <= [Quantity]");
                t.HasCheckConstraint("CK_Books_PublishYear", "[PublishYear] >= 1900 AND [PublishYear] <= 2026");
            });

            entity.HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasIndex(r => r.Email).IsUnique();
            entity.HasIndex(r => r.Phone).IsUnique();
        });

        modelBuilder.Entity<BorrowRecord>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_BorrowRecords_DueDate", "[DueDate] >= [BorrowDate]");
                t.HasCheckConstraint("CK_BorrowRecords_ReturnDate", "[ReturnDate] IS NULL OR [ReturnDate] >= [BorrowDate]");
            });

            entity.HasOne(br => br.Reader)
                .WithMany(r => r.BorrowRecords)
                .HasForeignKey(br => br.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BorrowDetail>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_BorrowDetails_Quantity", "[Quantity] >= 1 AND [Quantity] <= 50");
            });

            entity.HasIndex(bd => new { bd.BorrowRecordId, bd.BookId }).IsUnique();

            entity.HasOne(bd => bd.BorrowRecord)
                .WithMany(br => br.BorrowDetails)
                .HasForeignKey(bd => bd.BorrowRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bd => bd.Book)
                .WithMany(b => b.BorrowDetails)
                .HasForeignKey(bd => bd.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.SeedLibraryData();
    }
}
