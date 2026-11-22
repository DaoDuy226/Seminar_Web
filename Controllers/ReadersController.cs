using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

public class ReadersController : Controller
{
    private const string DatabaseErrorMessage =
        "Chưa kết nối được tới cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong README.md.";

    private readonly LibraryDbContext _context;
    private readonly ILogger<ReadersController> _logger;

    public ReadersController(LibraryDbContext context, ILogger<ReadersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Readers?search=...
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["CurrentFilter"] = search;
        var readerSummaries = new List<ReaderSummaryItem>();

        try
        {
            var query = _context.Readers
                .Include(r => r.BorrowRecords)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r => r.FullName.Contains(term) || r.Phone.Contains(term) || r.Email.Contains(term));
            }

            var readers = await query.OrderBy(r => r.Id).ToListAsync();
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
            ViewBag.DatabaseError = DatabaseErrorMessage;
        }

        return View(readerSummaries);
    }

    // GET: /Readers/Details/5  (hồ sơ + lịch sử mượn của độc giả)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (reader == null)
        {
            return NotFound();
        }

        ViewBag.BorrowRecords = await _context.BorrowRecords
            .Include(br => br.BorrowDetails)
                .ThenInclude(bd => bd.Book)
            .Where(br => br.ReaderId == id)
            .OrderByDescending(br => br.BorrowDate)
            .AsNoTracking()
            .ToListAsync();

        return View(reader);
    }

    // GET: /Readers/Create
    public IActionResult Create()
    {
        return View(new Reader());
    }

    // POST: /Readers/Create — chỉ bind các trường trên form (chống over-posting)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Phone,Email")] Reader reader)
    {
        await CheckDuplicateAsync(reader);
        if (!ModelState.IsValid)
        {
            return View(reader);
        }

        try
        {
            _context.Add(reader);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thêm độc giả \"{reader.FullName}\".";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể thêm độc giả {FullName}.", reader.FullName);
            ModelState.AddModelError(string.Empty, "Không thể lưu độc giả. Vui lòng kiểm tra dữ liệu và thử lại.");
        }

        return View(reader);
    }

    // GET: /Readers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers.FindAsync(id);
        return reader == null ? NotFound() : View(reader);
    }

    // POST: /Readers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Phone,Email")] Reader reader)
    {
        if (id != reader.Id)
        {
            return NotFound();
        }

        var existing = await _context.Readers.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await CheckDuplicateAsync(reader);
        if (!ModelState.IsValid)
        {
            return View(reader);
        }

        existing.FullName = reader.FullName;
        existing.Phone = reader.Phone;
        existing.Email = reader.Email;

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật độc giả \"{existing.FullName}\".";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể cập nhật độc giả {ReaderId}.", id);
            ModelState.AddModelError(string.Empty, "Không thể lưu thay đổi. Vui lòng thử lại.");
        }

        return View(reader);
    }

    // GET: /Readers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (reader == null)
        {
            return NotFound();
        }

        ViewBag.BorrowCount = await _context.BorrowRecords.CountAsync(br => br.ReaderId == id);
        return View(reader);
    }

    // POST: /Readers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reader = await _context.Readers.FindAsync(id);
        if (reader == null)
        {
            return NotFound();
        }

        // Không xóa độc giả đã có phiếu mượn để giữ nguyên lịch sử mượn trả.
        if (await _context.BorrowRecords.AnyAsync(br => br.ReaderId == id))
        {
            TempData["ErrorMessage"] = $"Không thể xóa độc giả \"{reader.FullName}\" vì độc giả đã có phiếu mượn trong hệ thống.";
            return RedirectToAction(nameof(Index));
        }

        _context.Readers.Remove(reader);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã xóa độc giả \"{reader.FullName}\".";
        return RedirectToAction(nameof(Index));
    }

    // Email và SĐT có unique index trong CSDL: báo lỗi thân thiện thay vì để SaveChanges ném exception.
    private async Task CheckDuplicateAsync(Reader reader)
    {
        if (!string.IsNullOrWhiteSpace(reader.Phone) &&
            await _context.Readers.AnyAsync(r => r.Phone == reader.Phone && r.Id != reader.Id))
        {
            ModelState.AddModelError(nameof(Reader.Phone), "Số điện thoại này đã được dùng cho độc giả khác.");
        }

        if (!string.IsNullOrWhiteSpace(reader.Email) &&
            await _context.Readers.AnyAsync(r => r.Email == reader.Email && r.Id != reader.Id))
        {
            ModelState.AddModelError(nameof(Reader.Email), "Email này đã được dùng cho độc giả khác.");
        }
    }
}
