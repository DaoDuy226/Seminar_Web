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

    public async Task<IActionResult> Books(string? search, int? categoryId, string availability = "all")
    {
        var vm = new BooksCatalogViewModel
        {
            SearchTerm = search?.Trim(),
            SelectedCategoryId = categoryId,
            AvailabilityFilter = availability
        };

        try
        {
            vm.Categories = await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();

            var query = _context.Books
                .Include(b => b.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
            {
                var term = vm.SearchTerm;
                query = query.Where(b =>
                    b.Title.Contains(term) ||
                    b.Author.Contains(term) ||
                    (b.Category != null && b.Category.Name.Contains(term)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (availability == "available")
            {
                query = query.Where(b => b.Available > 0);
            }
            else if (availability == "borrowed")
            {
                query = query.Where(b => b.Available < b.Quantity);
            }

            vm.Books = await query.OrderBy(b => b.Id).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh mục sách.");
            ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu.";
        }

        return View(vm);
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

    public async Task<IActionResult> Readers()
    {
        var readerSummaries = new List<ReaderSummaryItem>();

        try
        {
            var readers = await _context.Readers
                .Include(r => r.BorrowRecords)
                .OrderBy(r => r.Id)
                .AsNoTracking()
                .ToListAsync();

            readerSummaries = readers.Select(r => new ReaderSummaryItem
            {
                Reader = r,
                TotalBorrowCount = r.BorrowRecords.Count,
                ActiveBorrowCount = r.BorrowRecords.Count(br => br.ReturnDate == null),
                OverdueBorrowCount = r.BorrowRecords.Count(br => br.IsOverdue)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách độc giả.");
            ViewBag.DatabaseError = "Chưa kết nối được tới cơ sở dữ liệu.";
        }

        return View(readerSummaries);
    }

    public async Task<IActionResult> Borrows()
    {
        var borrowRecords = new List<BorrowRecord>();

        try
        {
            borrowRecords = await _context.BorrowRecords
                .Include(br => br.Reader)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
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

    public async Task<IActionResult> Statistics()
    {
        var model = await BuildSummaryAsync();
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
