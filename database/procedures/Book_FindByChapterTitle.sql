USE LibraryCatalogDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_FindByChapterTitle
    @Search nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT b.Id AS BookId, b.Title AS BookTitle,
           c.value('(@number)[1]', 'int') AS ChapterNumber,
           c.value('(@title)[1]', 'nvarchar(200)') AS ChapterTitle
    FROM dbo.Book AS b
    CROSS APPLY b.Contents.nodes('/Contents/Chapter') AS x(c)
    WHERE c.value('(@title)[1]', 'nvarchar(200)') LIKE N'%' + @Search + N'%'
    ORDER BY b.Title, ChapterNumber;
END;
GO
