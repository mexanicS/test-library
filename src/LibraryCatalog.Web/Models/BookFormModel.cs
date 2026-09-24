using System.ComponentModel.DataAnnotations;

namespace LibraryCatalog.Web.Models;

public sealed class BookFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название книги.")]
    [StringLength(200, ErrorMessage = "Не более 200 символов.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Укажите автора.")]
    [StringLength(200, ErrorMessage = "Не более 200 символов.")]
    public string Author { get; set; } = "";

    [Range(1450, 2100, ErrorMessage = "Год должен быть в диапазоне 1450–2100.")]
    public int? PublishedYear { get; set; }

    [StringLength(20, ErrorMessage = "Не более 20 символов.")]
    public string? Isbn { get; set; }

    [StringLength(2000, ErrorMessage = "Не более 2000 символов.")]
    public string? Description { get; set; }

    public List<ChapterInput> Chapters { get; set; } = [new()];

    public IFormFile? ContentsFile { get; set; }
}

public sealed class ChapterInput
{
    public string Title { get; set; } = "";

    public string BodyHtml { get; set; } = "";
}
