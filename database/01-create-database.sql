-- Run with sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database\01-create-database.sql
-- Or change the server name to your SQL Server Express instance.
IF DB_ID(N'LibraryCatalogDb') IS NULL
    CREATE DATABASE LibraryCatalogDb;
GO

USE LibraryCatalogDb;
GO

-- Требуется для методов xml и сохраняется в метаданных процедур.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.Book', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Book
    (
        Id            int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Book PRIMARY KEY,
        Title         nvarchar(200) NOT NULL,
        Author        nvarchar(200) NOT NULL,
        PublishedYear int NULL,
        Isbn          nvarchar(20) NULL,
        Description   nvarchar(2000) NULL,
        Contents      xml NOT NULL,
        CreatedAtUtc  datetime2(0) NOT NULL CONSTRAINT DF_Book_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc  datetime2(0) NOT NULL CONSTRAINT DF_Book_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Book_PublishedYear CHECK (PublishedYear IS NULL OR PublishedYear BETWEEN 1450 AND 2100),
        CONSTRAINT CK_Book_Title CHECK (LEN(TRIM(Title)) > 0),
        CONSTRAINT CK_Book_Author CHECK (LEN(TRIM(Author)) > 0)
    );
END;
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

CREATE OR ALTER PROCEDURE dbo.Book_ListChapters
    @BookId int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT b.Id AS BookId, b.Title AS BookTitle,
           c.value('(@number)[1]', 'int') AS ChapterNumber,
           c.value('(@title)[1]', 'nvarchar(200)') AS ChapterTitle
    FROM dbo.Book AS b
    CROSS APPLY b.Contents.nodes('/Contents/Chapter') AS x(c)
    WHERE b.Id = @BookId
    ORDER BY ChapterNumber;
END;
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
