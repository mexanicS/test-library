namespace LibraryCatalog.Web.Models;

public sealed record BookDetailsViewModel(BookRecord Book, IReadOnlyList<ChapterInput> Chapters);

public sealed record ChapterSearchViewModel(string Search, IReadOnlyList<ChapterMatch> Results);
