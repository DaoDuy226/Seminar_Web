
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Data;

public class ReadersController : Controller
{
    private readonly LibraryDbContext _context;

    public ReadersController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: READERS
    public async Task<IActionResult> Index(string searchString)
    {
        ViewData["CurrentFilter"] = searchString;

        var readersQuery = _context.Readers
            .Include(r => r.BorrowRecords)
                .ThenInclude(br => br.BorrowDetails)
            .AsQueryable();

        if (!String.IsNullOrEmpty(searchString))
        {
            readersQuery = readersQuery.Where(r => r.FullName.Contains(searchString) || r.Phone.Contains(searchString));
        }

        var readers = await readersQuery.ToListAsync();

        var readerSummaries = readers.Select(r => new LibraryManagement.Models.ViewModels.ReaderSummaryItem
        {
            Reader = r,
            TotalBorrowCount = r.BorrowRecords?.Count ?? 0,
            ActiveBorrowCount = r.BorrowRecords?.Count(br => br.ReturnDate == null) ?? 0,
            OverdueBorrowCount = r.BorrowRecords?.Count(br => br.ReturnDate == null && br.DueDate < DateTime.Now) ?? 0
        }).ToList();

        return View(readerSummaries);
    }

    // GET: READERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers
            .FirstOrDefaultAsync(m => m.Id == id);

        if (reader == null)
        {
            return NotFound();
        }

        // Nạp danh sách phiếu mượn kèm chi tiết sách của riêng độc giả này
        var borrowRecords = await _context.BorrowRecords
            .Include(br => br.BorrowDetails)
                .ThenInclude(bd => bd.Book)
            .Where(br => br.ReaderId == id)
            .ToListAsync();

        ViewBag.BorrowRecords = borrowRecords;

        return View(reader);
    }

    // GET: READERS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: READERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FullName,Phone,Email,BorrowRecords")] Reader reader)
    {
        if (ModelState.IsValid)
        {
            _context.Add(reader);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(reader);
    }

    // GET: READERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers.FindAsync(id);
        if (reader == null)
        {
            return NotFound();
        }
        return View(reader);
    }

    // POST: READERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,FullName,Phone,Email,BorrowRecords")] Reader reader)
    {
        if (id != reader.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(reader);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReaderExists(reader.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(reader);
    }

    // GET: READERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reader == null)
        {
            return NotFound();
        }

        return View(reader);
    }

    // POST: READERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var reader = await _context.Readers.FindAsync(id);
        if (reader == null)
        {
            return NotFound();
        }

        // Kiểm tra trực tiếp trong bảng BorrowRecords xem độc giả này có phiếu mượn nào không
        bool hasActiveBorrows = await _context.BorrowRecords.AnyAsync(br => br.ReaderId == id);

        if (hasActiveBorrows)
        {
            TempData["ErrorMessage"] = $"Không thể xóa độc giả '{reader.FullName}' vì thành viên này đang có phiếu mượn sách trong hệ thống!";
            return RedirectToAction(nameof(Index));
        }

        _context.Readers.Remove(reader);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã xóa độc giả thành công!";
        return RedirectToAction(nameof(Index));
    }

    private bool ReaderExists(int? id)
    {
        return _context.Readers.Any(e => e.Id == id);
    }
}
