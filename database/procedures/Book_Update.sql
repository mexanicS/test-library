USE LibraryCatalogDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_Update
    @Id int,
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

    UPDATE dbo.Book
    SET Title = TRIM(@Title),
        Author = TRIM(@Author),
        PublishedYear = @PublishedYear,
        Isbn = NULLIF(TRIM(@Isbn), N''),
        Description = NULLIF(TRIM(@Description), N''),
        Contents = @Contents,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50002, N'Книга не найдена.', 1;
END;
GO
