USE LibraryCatalogDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_List
    @Search nvarchar(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Title, Author, PublishedYear, Isbn, CreatedAtUtc, UpdatedAtUtc,
           Contents.value('count(/Contents/Chapter)', 'int') AS ChapterCount
    FROM dbo.Book
    WHERE @Search IS NULL OR @Search = N''
       OR Title LIKE N'%' + @Search + N'%'
       OR Author LIKE N'%' + @Search + N'%'
    ORDER BY Title, Id;
END;
GO
