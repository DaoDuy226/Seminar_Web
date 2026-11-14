namespace LibraryManagement.Models.ViewModels;

/// <summary>
/// ViewModel cho trang Dashboard và tổng hợp số liệu (#13, #14).
/// </summary>
public class DashboardViewModel
{
    /// <summary>Tổng số cuốn sách trong kho (SUM Quantity).</summary>
    public int TotalBooks { get; set; }

    /// <summary>Tổng số đầu sách khác nhau (COUNT Books).</summary>
    public int TotalBookTitles { get; set; }

    /// <summary>Tổng số thể loại sách.</summary>
    public int TotalCategories { get; set; }

    /// <summary>Tổng số độc giả đã đăng ký.</summary>
    public int TotalReaders { get; set; }

    /// <summary>Số phiếu đang mượn chưa trả (bao gồm cả đang trong hạn và quá hạn).</summary>
    public int ActiveBorrowCount { get; set; }

    /// <summary>Tổng số cuốn sách còn sẵn sàng trong kho (SUM Available).</summary>
    public int AvailableBooks { get; set; }

    /// <summary>Số phiếu đã quá hạn (ReturnDate == null &amp;&amp; DueDate &lt; Today).</summary>
    public int OverdueBorrowCount { get; set; }

    public List<Book> Books { get; set; } = new();
}
