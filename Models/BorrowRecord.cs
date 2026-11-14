using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models;

/// <summary>
/// Phiếu mượn sách của độc giả.
/// </summary>
public class BorrowRecord : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn độc giả.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn độc giả hợp lệ.")]
    [Display(Name = "Độc giả")]
    public int ReaderId { get; set; }

    public Reader? Reader { get; set; }

    [Required(ErrorMessage = "Ngày mượn không được để trống.")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày mượn")]
    public DateTime BorrowDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Ngày hẹn trả không được để trống.")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày hẹn trả")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    [DataType(DataType.Date)]
    [Display(Name = "Ngày trả thực tế")]
    public DateTime? ReturnDate { get; set; }

    [Required(ErrorMessage = "Trạng thái không được để trống.")]
    [StringLength(30, ErrorMessage = "Trạng thái không được vượt quá 30 ký tự.")]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = BorrowStatuses.Borrowing;

    public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();

    /// <summary>
    /// Cho biết phiếu mượn đã quá hạn hay chưa (chưa trả và ngày hẹn trả đã qua).
    /// </summary>
    [NotMapped]
    public bool IsOverdue => ReturnDate == null && DueDate.Date < DateTime.Today;

    /// <summary>
    /// Số ngày trễ hạn tính đến hôm nay (0 nếu chưa quá hạn hoặc đã trả).
    /// </summary>
    [NotMapped]
    public int OverdueDays => IsOverdue ? (DateTime.Today - DueDate.Date).Days : 0;

    /// <summary>
    /// Trạng thái thực tế tính theo ngày hiện tại để hiển thị đồng bộ ở Dashboard và Thống kê.
    /// </summary>
    [NotMapped]
    public string EffectiveStatus => ReturnDate != null
        ? BorrowStatuses.Returned
        : (IsOverdue ? BorrowStatuses.Overdue : BorrowStatuses.Borrowing);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate.Date < BorrowDate.Date)
        {
            yield return new ValidationResult(
                "Ngày hẹn trả không được trước ngày mượn.",
                new[] { nameof(DueDate) });
        }

        if (ReturnDate.HasValue && ReturnDate.Value.Date < BorrowDate.Date)
        {
            yield return new ValidationResult(
                "Ngày trả thực tế không được trước ngày mượn.",
                new[] { nameof(ReturnDate) });
        }
    }
}
