namespace LibraryManagement.Models.ViewModels;

/// <summary>
/// Số liệu đã tổng hợp ở controller để hiển thị trên trang thống kê và biểu đồ.
/// </summary>
public class StatisticsViewModel
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public int FirstAvailableYear { get; set; }
    public int CurrentYear { get; set; }
    public int LastSelectableMonth { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public string ComparisonPeriodLabel { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DashboardViewModel Summary { get; set; } = new();
    public int ReturnedBorrowCount { get; set; }
    public int OnTimeBorrowCount { get; set; }
    public int PeriodBorrowCount { get; set; }
    public int PreviousPeriodBorrowCount { get; set; }
    public int PeriodReturnCount { get; set; }
    public int PreviousPeriodReturnCount { get; set; }
    public int PeriodOverdueCount { get; set; }
    public int PreviousPeriodOverdueCount { get; set; }
    public List<string> ActivityLabels { get; set; } = new();
    public List<int> MonthlyBorrowCounts { get; set; } = new();
    public List<int> MonthlyReturnCounts { get; set; } = new();
    public List<int> MonthlyOverdueCounts { get; set; } = new();
    public List<string> InventoryLabels { get; set; } = new();
    public List<int> InventoryCounts { get; set; } = new();
    public List<PopularBookStatisticsItem> PopularBooks { get; set; } = new();
}

public class PopularBookStatisticsItem
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int CopiesBorrowed { get; set; }
    public int BorrowSlipCount { get; set; }
}
