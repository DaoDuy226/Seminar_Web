using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

[Route("borrows")]
public class BorrowsController(LibraryDbContext context, ILogger<BorrowsController> logger) : Controller
{
    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateBorrowViewModel
        {
            Quantity = 1,
            DueDate = DateTime.Today.AddDays(14)
        };

        return await ShowCreateAsync(model, cancellationToken);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ReaderId,BookId,Quantity,DueDate")] CreateBorrowViewModel model,
        CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        if (model.DueDate?.Date < today)
        {
            ModelState.AddModelError(nameof(model.DueDate), "Ngày hẹn trả không được trước ngày mượn.");
        }

        if (!ModelState.IsValid)
        {
            return await ShowCreateAsync(model, cancellationToken);
        }

        var quantity = model.Quantity!.Value;
        try
        {
            var readerExists = await context.Readers.AnyAsync(
                reader => reader.Id == model.ReaderId, cancellationToken);
            if (!readerExists)
            {
                ModelState.AddModelError(nameof(model.ReaderId), "Độc giả không còn trong danh sách.");
            }

            var book = await context.Books.SingleOrDefaultAsync(
                item => item.Id == model.BookId, cancellationToken);
            if (book is null)
            {
                ModelState.AddModelError(nameof(model.BookId), "Sách không còn trong danh mục.");
            }
            else if (book.Available < quantity)
            {
                ModelState.AddModelError(nameof(model.Quantity),
                    $"Sách không đủ số lượng. Kho hiện còn {book.Available} cuốn.");
            }

            if (!ModelState.IsValid || book is null)
            {
                return await ShowCreateAsync(model, cancellationToken);
            }

            var record = new BorrowRecord
            {
                ReaderId = model.ReaderId!.Value,
                BorrowDate = today,
                DueDate = model.DueDate!.Value.Date,
                Status = BorrowStatuses.Borrowing,
                BorrowDetails = [new BorrowDetail { BookId = book.Id, Quantity = quantity }]
            };

            book.Available -= quantity;
            context.BorrowRecords.Add(record);
            await context.SaveChangesAsync(cancellationToken);

            TempData["SuccessMessage"] = $"Đã lập phiếu mượn #{record.Id}.";
            return RedirectToAction(nameof(HomeController.Borrows), "Home");
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty, "Tồn kho vừa thay đổi. Vui lòng kiểm tra số lượng và lưu lại.");
        }
        catch (Exception ex) when (ex is DbUpdateException or System.Data.Common.DbException)
        {
            logger.LogError(ex, "Không thể lập phiếu mượn cho độc giả {ReaderId}.", model.ReaderId);
            ModelState.AddModelError(string.Empty, "Không thể lưu phiếu mượn. Vui lòng thử lại.");
        }

        return await ShowCreateAsync(model, cancellationToken);
    }

    [HttpPost("{id:int}/return")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
    {
        try
        {
            var record = await context.BorrowRecords
                .Include(item => item.BorrowDetails)
                    .ThenInclude(detail => detail.Book)
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (record is null)
            {
                return NotFound();
            }

            if (record.ReturnDate.HasValue)
            {
                TempData["ErrorMessage"] = $"Phiếu #{id} đã được trả.";
                return RedirectToAction(nameof(HomeController.Borrows), "Home");
            }

            var today = DateTime.Today;
            if (today < record.BorrowDate.Date || record.BorrowDetails.Count == 0 ||
                record.BorrowDetails.Any(detail => detail.Book is null ||
                    detail.Book.Available + detail.Quantity > detail.Book.Quantity))
            {
                TempData["ErrorMessage"] = $"Không thể trả phiếu #{id}. Vui lòng kiểm tra ngày mượn, chi tiết phiếu và tồn kho.";
                return RedirectToAction(nameof(HomeController.Borrows), "Home");
            }

            foreach (var detail in record.BorrowDetails)
            {
                detail.Book!.Available += detail.Quantity;
            }

            record.ReturnDate = today;
            record.Status = BorrowStatuses.Returned;
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = $"Đã xác nhận trả phiếu #{id}.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["ErrorMessage"] = "Phiếu hoặc tồn kho vừa thay đổi. Vui lòng kiểm tra lại trạng thái.";
        }
        catch (Exception ex) when (ex is DbUpdateException or System.Data.Common.DbException)
        {
            logger.LogError(ex, "Không thể xác nhận trả phiếu {BorrowId}.", id);
            TempData["ErrorMessage"] = "Không thể xác nhận trả sách. Vui lòng thử lại.";
        }

        return RedirectToAction(nameof(HomeController.Borrows), "Home");
    }

    private async Task<IActionResult> ShowCreateAsync(
        CreateBorrowViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            model.Readers = await context.Readers.AsNoTracking()
                .OrderBy(reader => reader.FullName).ThenBy(reader => reader.Id)
                .Select(reader => new SelectListItem(reader.FullName + " (" + reader.Phone + ")", reader.Id.ToString()))
                .ToListAsync(cancellationToken);

            model.Books = await context.Books.AsNoTracking()
                .OrderBy(book => book.Title).ThenBy(book => book.Id)
                .Select(book => new SelectListItem(
                    book.Title + " – " + book.Author + " (còn " + book.Available + ")", book.Id.ToString()))
                .ToListAsync(cancellationToken);
        }
        catch (System.Data.Common.DbException ex)
        {
            logger.LogError(ex, "Không thể tải danh sách độc giả và sách.");
            ModelState.AddModelError(string.Empty, "Không thể tải dữ liệu. Vui lòng kiểm tra kết nối rồi thử lại.");
        }

        return View("Create", model);
    }
}
