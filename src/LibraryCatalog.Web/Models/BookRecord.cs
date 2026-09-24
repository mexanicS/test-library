namespace LibraryCatalog.Web.Models;

public sealed class BookRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int? PublishedYear { get; set; }
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public string ContentsXml { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed record BookListItem(
    int Id, string Title, string Author, int? PublishedYear, string? Isbn,
    int ChapterCount, DateTime UpdatedAtUtc);

public sealed record ChapterMatch(int BookId, string BookTitle, int ChapterNumber, string ChapterTitle);
