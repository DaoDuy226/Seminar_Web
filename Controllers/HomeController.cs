using System.Diagnostics;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LibraryDbContext _context;

    public HomeController(ILogger<HomeController> logger, LibraryDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = await BuildSummaryAsync();
        if (model.TotalBookTitles > 0 || ViewBag.DatabaseError == null)
        {
            try
            {
                model.Books = await _context.Books
                    .Include(b => b.Category)
                    .OrderBy(b => b.Id)
                    .Take(8)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tải danh sách sách tiêu biểu.");
                ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong README.md.";
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Categories()
    {
        var categories = new List<Category>();
        try
        {
            categories = await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách thể loại.");
            ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu.";
        }

        return View(categories);
    }

    // GET: /Home/Borrows?search=...  (tìm theo mã phiếu, độc giả, SĐT hoặc tên sách)
    public async Task<IActionResult> Borrows(string? search)
    {
        ViewData["CurrentFilter"] = search;
        var borrowRecords = new List<BorrowRecord>();

        try
        {
            var query = _context.BorrowRecords
                .Include(br => br.Reader)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var hasId = int.TryParse(term.TrimStart('#'), out var borrowId);
                query = query.Where(br =>
                    (hasId && br.Id == borrowId) ||
                    br.Reader!.FullName.Contains(term) ||
                    br.Reader!.Phone.Contains(term) ||
                    br.BorrowDetails.Any(bd => bd.Book!.Title.Contains(term)));
            }

            borrowRecords = await query
                .OrderByDescending(br => br.BorrowDate)
                .ThenBy(br => br.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách phiếu mượn.");
            ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu.";
        }

        return View(borrowRecords);
    }

    public async Task<IActionResult> Overdue()
    {
        var overdueRecords = new List<BorrowRecord>();

        try
        {
            overdueRecords = await _context.BorrowRecords
                .Include(br => br.Reader)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .Where(br => br.ReturnDate == null && br.DueDate < DateTime.Today)
                .OrderBy(br => br.DueDate)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách phiếu quá hạn.");
            ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu.";
        }

        return View(overdueRecords);
    }

    public async Task<IActionResult> Statistics(int? year, int? month)
    {
        var today = DateTime.Today;
        var currentYear = today.Year;
        var selectedYear = Math.Clamp(year ?? currentYear, 2000, currentYear);
        var selectedMonth = month is >= 1 and <= 12 ? month : null;
        if (selectedYear == currentYear && selectedMonth.HasValue && selectedMonth.Value > today.Month)
        {
            selectedMonth = today.Month;
        }
        var periodStart = selectedMonth.HasValue
            ? new DateTime(selectedYear, selectedMonth.Value, 1)
            : new DateTime(selectedYear, 1, 1);
        var isPartialCurrentPeriod = selectedYear == currentYear &&
                                     (!selectedMonth.HasValue || selectedMonth.Value == today.Month);
        var periodEnd = isPartialCurrentPeriod
            ? today.AddDays(1)
            : selectedMonth.HasValue ? periodStart.AddMonths(1) : periodStart.AddYears(1);
        var previousPeriodStart = periodStart.AddYears(-1);
        var previousPeriodEnd = periodEnd.AddYears(-1);
        var previousPeriodLastDay = previousPeriodEnd.AddDays(-1);
        var model = new StatisticsViewModel
        {
            Year = selectedYear,
            Month = selectedMonth,
            FirstAvailableYear = Math.Max(2000, currentYear - 9),
            CurrentYear = currentYear,
            LastSelectableMonth = selectedYear == currentYear ? today.Month : 12,
            PeriodLabel = isPartialCurrentPeriod
                ? selectedMonth.HasValue
                    ? $"Tháng {selectedMonth:D2}/{selectedYear} đến {today:dd/MM/yyyy}"
                    : $"01/01/{selectedYear} – {today:dd/MM/yyyy}"
                : selectedMonth.HasValue ? $"Tháng {selectedMonth:D2}/{selectedYear}" : $"Năm {selectedYear}",
            ComparisonPeriodLabel = isPartialCurrentPeriod
                ? selectedMonth.HasValue
                    ? $"Tháng {selectedMonth:D2}/{selectedYear - 1} đến {previousPeriodLastDay:dd/MM/yyyy}"
                    : $"01/01/{selectedYear - 1} – {previousPeriodLastDay:dd/MM/yyyy}"
                : selectedMonth.HasValue ? $"Tháng {selectedMonth:D2}/{selectedYear - 1}" : $"Năm {selectedYear - 1}",
            GeneratedAt = DateTime.Now,
            InventoryLabels = new List<string> { "Có sẵn", "Đang được mượn" },
            Summary = await BuildSummaryAsync()
        };

        try
        {
            model.ReturnedBorrowCount = await _context.BorrowRecords
                .CountAsync(record => record.ReturnDate != null);
            model.OnTimeBorrowCount = await _context.BorrowRecords
                .CountAsync(record => record.ReturnDate == null && record.DueDate >= DateTime.Today);

            var bucketCount = selectedMonth.HasValue
                ? DateTime.DaysInMonth(selectedYear, selectedMonth.Value)
                : 12;
            model.ActivityLabels = Enumerable.Range(1, bucketCount)
                .Select(value => selectedMonth.HasValue ? value.ToString("00") : $"Tháng {value}")
                .ToList();

            var borrowCounts = await _context.BorrowRecords
                .Where(record => record.BorrowDate >= periodStart && record.BorrowDate < periodEnd)
                .GroupBy(record => selectedMonth.HasValue ? record.BorrowDate.Day : record.BorrowDate.Month)
                .Select(group => new { Bucket = group.Key, Count = group.Count() })
                .ToListAsync();

            var returnCounts = await _context.BorrowRecords
                .Where(record => record.ReturnDate.HasValue &&
                                 record.ReturnDate.Value >= periodStart &&
                                 record.ReturnDate.Value < periodEnd)
                .GroupBy(record => selectedMonth.HasValue ? record.ReturnDate!.Value.Day : record.ReturnDate!.Value.Month)
                .Select(group => new { Bucket = group.Key, Count = group.Count() })
                .ToListAsync();

            var overdueCounts = await _context.BorrowRecords
                .Where(record => record.ReturnDate == null &&
                                 record.DueDate < DateTime.Today &&
                                 record.DueDate >= periodStart &&
                                 record.DueDate < periodEnd)
                .GroupBy(record => selectedMonth.HasValue ? record.DueDate.Day : record.DueDate.Month)
                .Select(group => new { Bucket = group.Key, Count = group.Count() })
                .ToListAsync();

            model.MonthlyBorrowCounts = Enumerable.Range(1, bucketCount)
                .Select(bucket => borrowCounts.FirstOrDefault(item => item.Bucket == bucket)?.Count ?? 0)
                .ToList();
            model.MonthlyReturnCounts = Enumerable.Range(1, bucketCount)
                .Select(bucket => returnCounts.FirstOrDefault(item => item.Bucket == bucket)?.Count ?? 0)
                .ToList();
            model.MonthlyOverdueCounts = Enumerable.Range(1, bucketCount)
                .Select(bucket => overdueCounts.FirstOrDefault(item => item.Bucket == bucket)?.Count ?? 0)
                .ToList();

            model.PeriodBorrowCount = model.MonthlyBorrowCounts.Sum();
            model.PreviousPeriodBorrowCount = await _context.BorrowRecords
                .CountAsync(record => record.BorrowDate >= previousPeriodStart && record.BorrowDate < previousPeriodEnd);
            model.PeriodReturnCount = model.MonthlyReturnCounts.Sum();
            model.PreviousPeriodReturnCount = await _context.BorrowRecords
                .CountAsync(record => record.ReturnDate.HasValue &&
                                      record.ReturnDate.Value >= previousPeriodStart &&
                                      record.ReturnDate.Value < previousPeriodEnd);
            model.PeriodOverdueCount = model.MonthlyOverdueCounts.Sum();
            model.PreviousPeriodOverdueCount = await _context.BorrowRecords
                .CountAsync(record => record.ReturnDate == null &&
                                      record.DueDate < DateTime.Today &&
                                      record.DueDate >= previousPeriodStart &&
                                      record.DueDate < previousPeriodEnd);

            model.PopularBooks = await _context.BorrowDetails
                .Where(detail => detail.BorrowRecord!.BorrowDate >= periodStart &&
                                 detail.BorrowRecord.BorrowDate < periodEnd)
                .GroupBy(detail => new { detail.BookId, detail.Book!.Title, detail.Book.Author })
                .Select(group => new PopularBookStatisticsItem
                {
                    BookId = group.Key.BookId,
                    Title = group.Key.Title,
                    Author = group.Key.Author,
                    CopiesBorrowed = group.Sum(detail => detail.Quantity),
                    BorrowSlipCount = group.Select(detail => detail.BorrowRecordId).Distinct().Count()
                })
                .OrderByDescending(book => book.CopiesBorrowed)
                .ThenBy(book => book.Title)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            model.InventoryCounts = new List<int>
            {
                model.Summary.AvailableBooks,
                Math.Max(0, model.Summary.TotalBooks - model.Summary.AvailableBooks)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải số liệu trang thống kê từ SQL Server.");
            ViewBag.DatabaseError ??= "Chưa kết nối được tới cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong README.md.";
        }

        return View(model);
    }

    private async Task<DashboardViewModel> BuildSummaryAsync()
    {
        var model = new DashboardViewModel();

        try
        {
            model.TotalCategories = await _context.Categories.CountAsync();
            model.TotalBookTitles = await _context.Books.CountAsync();
            model.TotalBooks = await _context.Books.SumAsync(b => (int?)b.Quantity) ?? 0;
            model.AvailableBooks = await _context.Books.SumAsync(b => (int?)b.Available) ?? 0;
            model.TotalReaders = await _context.Readers.CountAsync();
            model.ActiveBorrowCount = await _context.BorrowRecords.CountAsync(br => br.ReturnDate == null);
            model.OverdueBorrowCount = await _context.BorrowRecords
                .CountAsync(br => br.ReturnDate == null && br.DueDate < DateTime.Today);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải dữ liệu từ SQL Server.");
            ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong README.md.";
        }

        return model;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
