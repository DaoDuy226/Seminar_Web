# P4. View & Razor

> Ví dụ trích từ demo Quản lý Thư viện. Người phụ trách: Trương Sĩ Duy

## 1. Razor syntax
**Ý chính:** Razor cho phép viết C# trong HTML bằng ký tự `@`. Mọi giá trị in ra bằng `@` đều tự động được HTML-encode, nhờ đó chống XSS (xem P5). Riêng `@Html.Raw()` không encode, nên phải dùng cẩn thận.

| Cú pháp | Ví dụ trong demo |
|---|---|
| Biểu thức | `@book.Title` |
| Biểu thức phức tạp | `@(book.Available > 0 ? "text-bg-success" : "text-bg-danger")` |
| Vòng lặp / điều kiện | `@foreach (var book in Model.Books)`, `@if (Model.Books.Count == 0)` |
| Null-safe | `@book.Category?.Name` |
| Khối code | `@{ ViewData["Title"] = "Kho sách"; }` |
| Comment | `@* không xuất hiện trong HTML gửi về trình duyệt *@` |

File: `Views/Home/Books.cshtml`

## 2. Layout
**Ý chính:** Layout là khung chung của mọi trang (navbar, footer, CSS/JS). Mỗi View chỉ chứa phần nội dung riêng.

- `Views/Shared/_Layout.cshtml`
  - `@RenderBody()`: chỗ chèn nội dung của View con
  - `@await RenderSectionAsync("Scripts", required: false)`: chỗ để View con chèn thêm script riêng, không bắt buộc
  - `<title>@ViewData["Title"] - Thư Viện Số SE347</title>`: View con truyền tiêu đề lên Layout
- `Views/_ViewStart.cshtml`: `@{ Layout = "_Layout"; }` chạy trước mọi View, gán Layout mặc định
- `Views/_ViewImports.cshtml`: khai báo `@using` và `@addTagHelper` dùng chung cho mọi View

## 3. Partial View
**Ý chính:** Partial là một mảnh giao diện dùng lại được, giúp tránh copy-paste (nguyên tắc DRY).

**Ví dụ thực tế:** khối báo lỗi kết nối CSDL trước đây bị lặp ở 7 View, nay gom về 1 file:

```cshtml
@* Views/Shared/_DatabaseErrorAlert.cshtml *@
@if (ViewBag.DatabaseError != null)
{
    <div class="alert alert-danger border-2 shadow-sm mb-4" role="alert">
        <strong>Lỗi kết nối CSDL:</strong> @ViewBag.DatabaseError
    </div>
}
```
Cách gọi trong View: `<partial name="_DatabaseErrorAlert" />`

- Partial dùng chung ViewData/ViewBag với View cha. Có thể truyền model riêng: `<partial name="_X" model="book" />`
- Phân biệt với **View Component**: View Component có class C# riêng, tự truy vấn được dữ liệu. Nó hợp với các khối độc lập như ô thống kê hay giỏ hàng.

## 4. Tag Helpers
**Ý chính:** Tag Helper là các thuộc tính `asp-*` gắn trên thẻ HTML và được xử lý ở server. Chúng sinh URL đúng theo routing, đọc giống HTML hơn HTML Helper (`@Html.ActionLink`), và có IntelliSense.

| Tag Helper | Ví dụ trong demo | HTML sinh ra |
|---|---|---|
| Anchor | `<a asp-controller="Home" asp-action="Books">` | `<a href="/Home/Books">` |
| Route value | `asp-route-categoryId="@book.CategoryId"` | `href="/Home/Books?categoryId=3"` |
| Form | `<form method="get" asp-controller="Home" asp-action="Books">` | `<form action="/Home/Books">` |
| Cache busting | `<link href="~/css/site.css" asp-append-version="true">` | `site.css?v=abc123` |
| Partial | `<partial name="_DatabaseErrorAlert" />` | (nội dung partial) |

Ngoài ra: `asp-for` / `asp-validation-for` gắn input với property của Model và tự sinh validation từ Data Annotations (P3). Form `method="post"` tự chèn token chống CSRF (P5).

## 5. ViewModel vs ViewBag / ViewData
| | ViewData | ViewBag | ViewModel |
|---|---|---|---|
| Kiểu | Dictionary: `ViewData["Title"]` | dynamic: `ViewBag.DatabaseError` | Class: `DashboardViewModel` |
| Kiểm tra lúc build | ✗ | ✗ | ✓ |
| IntelliSense | ✗ | ✗ | ✓ |
| Dùng cho | Dữ liệu phụ, nhỏ | Dữ liệu phụ, nhỏ | Dữ liệu chính của trang |

**Ví dụ trong `Views/Home/Index.cshtml`** (dùng cả 3 cách):
```cshtml
@model DashboardViewModel                   @* ViewModel: dữ liệu chính *@
@{ ViewData["Title"] = "Dashboard"; }       @* ViewData: tiêu đề cho Layout *@
<partial name="_DatabaseErrorAlert" />      @* bên trong dùng ViewBag.DatabaseError *@
...
<div class="fs-2 fw-bold">@Model.TotalBooks</div>
```
`DashboardViewModel` (`Models/ViewModels/`) gom số liệu từ nhiều bảng: TotalBooks, TotalReaders, ActiveBorrowCount, OverdueBorrowCount... Đó là những thứ một Entity đơn lẻ không chứa được.

**Kết luận:** dùng ViewModel cho dữ liệu chính; ViewBag/ViewData chỉ cho dữ liệu phụ.

## Câu hỏi vấn đáp dự kiến
1. Partial View khác View Component? → Partial chỉ là giao diện. View Component có logic C# riêng và tự lấy dữ liệu.
2. Vì sao không truyền thẳng Entity ra View? → View có thể cần dữ liệu từ nhiều bảng; tránh lộ field nhạy cảm; tránh over-posting khi nhận form.
3. `_ViewStart` khác `_ViewImports`? → ViewStart **chạy code** (gán Layout). ViewImports **khai báo** using và Tag Helper.
4. `@Html.Raw` nguy hiểm thế nào? → Không encode HTML, nên có thể bị chèn script (XSS).
5. ViewBag khác ViewData? → Cùng một kho dữ liệu, chỉ khác cú pháp. ViewBag là dynamic, ViewData là dictionary.

## Nguồn tham khảo
- https://learn.microsoft.com/aspnet/core/mvc/views/razor
- https://learn.microsoft.com/aspnet/core/mvc/views/layout
- https://learn.microsoft.com/aspnet/core/mvc/views/partial
- https://learn.microsoft.com/aspnet/core/mvc/views/tag-helpers/intro
- https://learn.microsoft.com/aspnet/core/mvc/views/overview