using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

public class BooksController : Controller
{
    private const string DatabaseErrorMessage =
        "Chưa kết nối được tới cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong README.md.";

    private readonly LibraryDbContext _context;
    private readonly ILogger<BooksController> _logger;

    public BooksController(LibraryDbContext context, ILogger<BooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Books?search=...&categoryId=...&availability=all|available|borrowed
    public async Task<IActionResult> Index(string? search, int? categoryId, string availability = "all")
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
                .OrderBy(c => c.Name)
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

            if (categoryId is > 0)
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
            ViewBag.DatabaseError = DatabaseErrorMessage;
        }

        return View(vm);
    }

    // GET: /Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return book == null ? NotFound() : View(book);
    }

    // GET: /Books/Create
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new Book { Quantity = 1, PublishYear = DateTime.Today.Year });
    }

    // POST: /Books/Create
    // Chỉ bind các trường người dùng được nhập (chống over-posting).
    // Sách mới chưa ai mượn nên Available = Quantity, do server tự gán.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Author,CategoryId,Quantity,PublishYear")] Book book)
    {
        book.Available = book.Quantity;

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(book.CategoryId);
            return View(book);
        }

        try
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thêm sách \"{book.Title}\".";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể thêm sách {Title}.", book.Title);
            ModelState.AddModelError(string.Empty, "Không thể lưu sách. Vui lòng kiểm tra dữ liệu và thử lại.");
        }

        await LoadCategoriesAsync(book.CategoryId);
        return View(book);
    }

    // GET: /Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        await LoadCategoriesAsync(book.CategoryId);
        return View(book);
    }

    // POST: /Books/Edit/5
    // Nạp bản ghi từ CSDL rồi gán từng trường (không dùng Update(book)),
    // để EF so sánh đúng giá trị gốc của Available/Quantity (concurrency token)
    // và để số sách đang được mượn không bị ghi đè.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,CategoryId,Quantity,PublishYear")] Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        var existing = await _context.Books.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        var borrowedCopies = existing.Quantity - existing.Available;
        if (book.Quantity < borrowedCopies)
        {
            ModelState.AddModelError(nameof(Book.Quantity),
                $"Đang có {borrowedCopies} cuốn được mượn, tổng số lượng không được nhỏ hơn {borrowedCopies}.");
        }

        // Available không có trên form: hiển thị lại giá trị hiện tại khi phải trả về form.
        book.Available = existing.Available;
        ModelState.Remove(nameof(Book.Available));

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(book.CategoryId);
            return View(book);
        }

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.CategoryId = book.CategoryId;
        existing.PublishYear = book.PublishYear;
        existing.Quantity = book.Quantity;
        existing.Available = book.Quantity - borrowedCopies;

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật sách \"{existing.Title}\".";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty,
                "Tồn kho của sách vừa thay đổi (có phiếu mượn/trả mới). Vui lòng tải lại trang và sửa lại.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể cập nhật sách {BookId}.", id);
            ModelState.AddModelError(string.Empty, "Không thể lưu thay đổi. Vui lòng thử lại.");
        }

        await LoadCategoriesAsync(book.CategoryId);
        return View(book);
    }

    // GET: /Books/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return book == null ? NotFound() : View(book);
    }

    // POST: /Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books
            .Include(b => b.BorrowDetails)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        // Không xóa sách đã xuất hiện trong bất kỳ phiếu mượn nào (kể cả phiếu đã trả)
        // để giữ nguyên lịch sử mượn trả.
        if (book.BorrowDetails.Any())
        {
            TempData["ErrorMessage"] = $"Không thể xóa sách \"{book.Title}\" vì sách đã có trong lịch sử phiếu mượn.";
            return RedirectToAction(nameof(Index));
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã xóa sách \"{book.Title}\".";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync(int? selectedId = null)
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).AsNoTracking().ToListAsync();
        ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", selectedId);
    }
}
