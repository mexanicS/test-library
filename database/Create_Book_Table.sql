USE LibraryCatalogDb;
GO

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
