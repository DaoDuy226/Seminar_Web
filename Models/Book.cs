using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

/// <summary>
/// Đầu sách trong thư viện, gắn Data Annotations để demo validation khi nhập liệu sai.
/// </summary>
public class Book : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sách không được để trống.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sách phải từ 2 đến 200 ký tự.")]
    [Display(Name = "Tên sách")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tác giả không được để trống.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên tác giả phải từ 2 đến 150 ký tự.")]
    [Display(Name = "Tác giả")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn thể loại.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thể loại hợp lệ.")]
    [Display(Name = "Thể loại")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required(ErrorMessage = "Tổng số lượng không được để trống.")]
    [Range(1, 1000, ErrorMessage = "Tổng số lượng phải từ 1 đến 1000.")]
    [Display(Name = "Tổng số lượng")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Số lượng còn lại không được để trống.")]
    [Range(0, 1000, ErrorMessage = "Số lượng còn lại phải từ 0 đến 1000.")]
    [Display(Name = "Số lượng còn lại")]
    public int Available { get; set; }

    [Required(ErrorMessage = "Năm xuất bản không được để trống.")]
    [Range(1900, 2026, ErrorMessage = "Năm xuất bản phải từ 1900 đến 2026.")]
    [Display(Name = "Năm xuất bản")]
    public int PublishYear { get; set; }

    public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Available > Quantity)
        {
            yield return new ValidationResult(
                "Số lượng còn lại không được lớn hơn tổng số lượng.",
                new[] { nameof(Available) });
        }
    }
}
