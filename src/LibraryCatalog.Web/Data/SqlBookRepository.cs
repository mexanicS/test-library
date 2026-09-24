using System.Data;
using LibraryCatalog.Web.Models;
using Microsoft.Data.SqlClient;

namespace LibraryCatalog.Web.Data;

public sealed class SqlBookRepository(IConfiguration configuration) : IBookRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("Library")
        ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Library.");

    public async Task<IReadOnlyList<BookListItem>> ListAsync(string? search, CancellationToken cancellationToken)
    {
        var result = new List<BookListItem>();
        
        await using var connection = new SqlConnection(_connectionString);
        await using var command = Procedure(connection, "dbo.Book_List");
        
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = DbValue(search);
        
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new BookListItem(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(7), reader.GetDateTime(6)));
        }
        
        return result;
    }

    public async Task<BookRecord?> GetAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = Procedure(connection, "dbo.Book_GetById");
        
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BookRecord
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Author = reader.GetString(2),
            PublishedYear = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            Isbn = reader.IsDBNull(4) ? null : reader.GetString(4),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5),
            ContentsXml = reader.GetSqlXml(6).Value,
            CreatedAtUtc = reader.GetDateTime(7),
            UpdatedAtUtc = reader.GetDateTime(8)
        };
    }

    public async Task<int> CreateAsync(BookRecord book, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = Procedure(connection, "dbo.Book_Create");
        
        AddBookParameters(command, book);
        
        await connection.OpenAsync(cancellationToken);
        
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateAsync(BookRecord book, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = Procedure(connection, "dbo.Book_Update");
        
        command.Parameters.Add("@Id", SqlDbType.Int).Value = book.Id;
        
        AddBookParameters(command, book);
        
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = Procedure(connection, "dbo.Book_Delete");
        
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChapterMatch>> FindChaptersAsync(string search, CancellationToken cancellationToken)
    {
        var result = new List<ChapterMatch>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = Procedure(connection, "dbo.Book_FindByChapterTitle");
        
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = search;
        
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new ChapterMatch(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3)));
        
        return result;
    }

    private static SqlCommand Procedure(SqlConnection connection, string name) =>
        new(name, connection) { CommandType = CommandType.StoredProcedure };

    private static object DbValue(object? value) => value ?? DBNull.Value;

    private static void AddBookParameters(SqlCommand command, BookRecord book)
    {
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = book.Title;
        command.Parameters.Add("@Author", SqlDbType.NVarChar, 200).Value = book.Author;
        command.Parameters.Add("@PublishedYear", SqlDbType.Int).Value = DbValue(book.PublishedYear);
        command.Parameters.Add("@Isbn", SqlDbType.NVarChar, 20).Value = DbValue(book.Isbn);
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 2000).Value = DbValue(book.Description);
        command.Parameters.Add("@Contents", SqlDbType.Xml).Value = book.ContentsXml;
    }
}
