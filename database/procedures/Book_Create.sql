USE LibraryCatalogDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_Create
    @Title nvarchar(200),
    @Author nvarchar(200),
    @PublishedYear int = NULL,
    @Isbn nvarchar(20) = NULL,
    @Description nvarchar(2000) = NULL,
    @Contents xml
AS
BEGIN
    SET NOCOUNT ON;
    IF @Contents IS NULL OR @Contents.exist('/Contents[1]') <> 1
        THROW 50001, N'Оглавление должно иметь корневой элемент Contents.', 1;

    INSERT dbo.Book (Title, Author, PublishedYear, Isbn, Description, Contents)
    VALUES (TRIM(@Title), TRIM(@Author), @PublishedYear, NULLIF(TRIM(@Isbn), N''),
            NULLIF(TRIM(@Description), N''), @Contents);

    SELECT CONVERT(int, SCOPE_IDENTITY()) AS Id;
END;
GO
