using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

/// <summary>
/// Thể loại sách trong thư viện.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên thể loại không được để trống.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên thể loại phải từ 2 đến 100 ký tự.")]
    [Display(Name = "Tên thể loại")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
