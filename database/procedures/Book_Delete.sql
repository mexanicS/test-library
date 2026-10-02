USE LibraryCatalogDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_Delete
    @Id int
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Book WHERE Id = @Id;
    IF @@ROWCOUNT = 0
        THROW 50002, N'Книга не найдена.', 1;
END;
GO
