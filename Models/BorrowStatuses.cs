namespace LibraryManagement.Models;

/// <summary>
/// Hằng số trạng thái cho phiếu mượn sách (BorrowRecord.Status).
/// Dùng thống nhất giữa Models, SeedData, Controllers và Views.
/// </summary>
public static class BorrowStatuses
{
    public const string Borrowing = "Đang mượn";
    public const string Returned = "Đã trả";
    public const string Overdue = "Quá hạn";
}
