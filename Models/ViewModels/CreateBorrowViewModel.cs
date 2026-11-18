using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Models.ViewModels;

public class CreateBorrowViewModel
{
    [Display(Name = "Độc giả")]
    [Required(ErrorMessage = "Vui lòng chọn độc giả.")]
    [Range(1, int.MaxValue, ErrorMessage = "Độc giả không hợp lệ.")]
    public int? ReaderId { get; set; }

    [Display(Name = "Sách")]
    [Required(ErrorMessage = "Vui lòng chọn sách.")]
    [Range(1, int.MaxValue, ErrorMessage = "Sách không hợp lệ.")]
    public int? BookId { get; set; }

    [Display(Name = "Số lượng")]
    [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
    [Range(1, 50, ErrorMessage = "Số lượng mượn phải từ 1 đến 50.")]
    public int? Quantity { get; set; }

    [Display(Name = "Ngày hẹn trả")]
    [Required(ErrorMessage = "Vui lòng nhập ngày hẹn trả.")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    public IReadOnlyList<SelectListItem> Readers { get; set; } = [];
    public IReadOnlyList<SelectListItem> Books { get; set; } = [];
}
