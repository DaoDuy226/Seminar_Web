namespace LibraryManagement.Models.ViewModels;

public class BooksCatalogViewModel
{
    public string? SearchTerm { get; set; }
    public int? SelectedCategoryId { get; set; }
    public string AvailabilityFilter { get; set; } = "all";

    public List<Category> Categories { get; set; } = new();
    public List<Book> Books { get; set; } = new();
}

public class ReaderSummaryItem
{
    public Reader Reader { get; set; } = new();
    public int TotalBorrowCount { get; set; }
    public int ActiveBorrowCount { get; set; }
    public int OverdueBorrowCount { get; set; }
}
