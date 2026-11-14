using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

/// <summary>
/// Thông tin độc giả mượn sách.
/// </summary>
public class Reader
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [StringLength(15, MinimumLength = 9, ErrorMessage = "Số điện thoại phải từ 9 đến 15 ký tự.")]
    [RegularExpression(@"^0\d{8,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và gồm 9 đến 11 chữ số.")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
}
