USE LibraryCatalogDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_GetById
    @Id int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Author, PublishedYear, Isbn, Description, Contents,
           CreatedAtUtc, UpdatedAtUtc
    FROM dbo.Book
    WHERE Id = @Id;
END;
GO
