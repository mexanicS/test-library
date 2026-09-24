using LibraryCatalog.Web.Models;

namespace LibraryCatalog.Web.Data;

public interface IBookRepository
{
    Task<IReadOnlyList<BookListItem>> ListAsync(string? search, CancellationToken cancellationToken);
    
    Task<BookRecord?> GetAsync(int id, CancellationToken cancellationToken);
    
    Task<int> CreateAsync(BookRecord book, CancellationToken cancellationToken);
    
    Task UpdateAsync(BookRecord book, CancellationToken cancellationToken);
    
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<ChapterMatch>> FindChaptersAsync(string search, CancellationToken cancellationToken);
}
