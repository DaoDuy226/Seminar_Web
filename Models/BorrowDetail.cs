using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

/// <summary>
/// Chi tiết từng đầu sách và số lượng trong một phiếu mượn.
/// </summary>
public class BorrowDetail
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Phiếu mượn không được để trống.")]
    [Display(Name = "Phiếu mượn")]
    public int BorrowRecordId { get; set; }

    public BorrowRecord? BorrowRecord { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn sách.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sách hợp lệ.")]
    [Display(Name = "Sách")]
    public int BookId { get; set; }

    public Book? Book { get; set; }

    [Required(ErrorMessage = "Số lượng mượn không được để trống.")]
    [Range(1, 50, ErrorMessage = "Số lượng mượn mỗi đầu sách phải từ 1 đến 50.")]
    [Display(Name = "Số lượng mượn")]
    public int Quantity { get; set; } = 1;
}
