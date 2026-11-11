CREATE PROCEDURE [dbo].[SeedDatabase]
@Categories       INT = 50,
@Suppliers        INT = 500,
@Customers        INT = 20000,
@Products         INT = 10000,
@Orders           INT = 50000,
@MaxItemsPerOrder INT = 7,
@Reset            BIT = 0
AS
BEGIN

IF @Reset = 0 AND EXISTS (SELECT 1 FROM dbo.[Product]) RETURN;

IF @Reset = 1
    BEGIN
        DELETE FROM dbo.OrderItem;
        DELETE FROM dbo.[Order];
        DELETE FROM dbo.[Product];
        DELETE FROM dbo.Customer;
        DELETE FROM dbo.Addressee;
        DELETE FROM dbo.Supplier;
        DELETE FROM dbo.Category;
    END

-------------------------------------------------------------------
-- 1) Névlisták
-------------------------------------------------------------------
DECLARE @FirstNames TABLE (Idx INT IDENTITY(0,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @FirstNames (Name) VALUES
(N'Anna'),(N'Béla'),(N'Csaba'),(N'Dóra'),(N'Emese'),(N'Ferenc'),(N'Gábor'),(N'Hanna'),(N'István'),(N'Judit'),
(N'Katalin'),(N'László'),(N'Mária'),(N'Nándor'),(N'Orsolya'),(N'Péter'),(N'Réka'),(N'Sándor'),(N'Tamás'),(N'Zsófia');

DECLARE @LastNames TABLE (Idx INT IDENTITY(0,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @LastNames (Name) VALUES
(N'Kovács'),(N'Szabó'),(N'Tóth'),(N'Nagy'),(N'Horváth'),(N'Varga'),(N'Kiss'),(N'Molnár'),(N'Németh'),(N'Farkas'),
(N'Balogh'),(N'Papp'),(N'Takács'),(N'Juhász'),(N'Lakatos'),(N'Mészáros'),(N'Oláh'),(N'Simon'),(N'Rácz'),(N'Fekete');

DECLARE @Cities TABLE (Idx INT IDENTITY(0,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @Cities (Name) VALUES
(N'Budapest'),(N'Debrecen'),(N'Szeged'),(N'Miskolc'),(N'Pécs'),(N'Győr'),(N'Nyíregyháza'),(N'Kecskemét'),
(N'Székesfehérvár'),(N'Szombathely'),(N'Sopron'),(N'Eger'),(N'Zalaegerszeg'),(N'Kaposvár'),(N'Veszprém');

DECLARE @Countries TABLE (Idx INT IDENTITY(0,1) PRIMARY KEY, Code NVARCHAR(2));
INSERT INTO @Countries (Code) VALUES
(N'HU'),(N'DE'),(N'AT'),(N'SK'),(N'RO'),(N'PL'),(N'CZ'),(N'SI'),(N'HR'),(N'IT');

DECLARE @ProductAdjectives TABLE (Idx INT IDENTITY(0,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @ProductAdjectives (Name) VALUES
(N'Prémium'),(N'Klasszikus'),(N'Kompakt'),(N'Professzionális'),(N'Gazdaságos'),(N'Extra'),(N'Digitális'),
(N'Hordozható'),(N'Vezeték nélküli'),(N'Összecsukható');

DECLARE @ProductNouns TABLE (Idx INT IDENTITY(0,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @ProductNouns (Name) VALUES
(N'kávéfőző'),(N'monitor'),(N'billentyűzet'),(N'szék'),(N'asztal'),(N'lámpa'),(N'hangszóró'),(N'táska'),
(N'egér'),(N'kamera'),(N'nyomtató'),(N'polc'),(N'ventilátor'),(N'fejhallgató'),(N'dokkoló');

-------------------------------------------------------------------
-- 2) Számsorozat-generátor
-------------------------------------------------------------------
DECLARE @MaxN INT = (SELECT MAX(v) FROM (VALUES
    (@Categories), (@Suppliers), (@Customers), (@Products), (@Orders)
) AS x(v));

SELECT TOP (@MaxN)
       ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
INTO #Numbers
FROM sys.all_objects a
CROSS JOIN sys.all_objects b;

CREATE UNIQUE CLUSTERED INDEX IX_Numbers ON #Numbers(N);

-------------------------------------------------------------------
-- 3) Category
-------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Category ON;

INSERT INTO dbo.Category (Id, Name, Description, IsActive)
SELECT
    n.N,
    N'Kategória ' + RIGHT('000' + CAST(n.N AS VARCHAR(3)), 3),
    N'A(z) ' + RIGHT('000' + CAST(n.N AS VARCHAR(3)), 3) + N'. termékkategória leírása.',
    CASE WHEN ABS(CHECKSUM(NEWID())) % 10 <> 0 THEN 1 ELSE 0 END
FROM #Numbers n
WHERE n.N <= @Categories;

SET IDENTITY_INSERT dbo.Category OFF;

-------------------------------------------------------------------
-- 4) Supplier
-------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Supplier ON;

INSERT INTO dbo.Supplier (Id, LastName, Email, PhoneNumber, IsActive)
SELECT
    n.N,
    (SELECT Name FROM @LastNames WHERE Idx = ABS(CHECKSUM(NEWID())) % 20)
        + N' Kft. ' + RIGHT('0000' + CAST(n.N AS VARCHAR(4)), 4),
    N'beszallito' + CAST(n.N AS VARCHAR(10)) + N'@example.com',
    N'+36 ' + CAST(20 + ABS(CHECKSUM(NEWID())) % 51 AS VARCHAR(2))
        + N' ' + CAST(1000000 + ABS(CHECKSUM(NEWID())) % 8999999 AS VARCHAR(7)),
    CASE WHEN ABS(CHECKSUM(NEWID())) % 10 <> 0 THEN 1 ELSE 0 END
FROM #Numbers n
WHERE n.N <= @Suppliers;

SET IDENTITY_INSERT dbo.Supplier OFF;

-------------------------------------------------------------------
-- 5) Addressee
-------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Addressee ON;

INSERT INTO dbo.Addressee (Id, AddresseeCountry, AddresseeZip, AddresseeAddress, AddresseeCity)
SELECT
    n.N,
    (SELECT Code FROM @Countries WHERE Idx = ABS(CHECKSUM(NEWID())) % 10),
    CAST(1000 + ABS(CHECKSUM(NEWID())) % 8999 AS VARCHAR(4)),
    N'Fő utca ' + CAST(1 + ABS(CHECKSUM(NEWID())) % 199 AS VARCHAR(3)) + N'.',
    (SELECT Name FROM @Cities WHERE Idx = ABS(CHECKSUM(NEWID())) % 15)
FROM #Numbers n
WHERE n.N <= @Customers;

SET IDENTITY_INSERT dbo.Addressee OFF;

-------------------------------------------------------------------
-- 6) Customer
-------------------------------------------------------------------
SET IDENTITY_INSERT dbo.Customer ON;

INSERT INTO dbo.Customer (Id, CreditedAt, FirstName, LastName, Email, PhoneNumber, AddresseeId)
SELECT
    n.N,
    DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 1825), SYSUTCDATETIME()),
    (SELECT Name FROM @FirstNames WHERE Idx = ABS(CHECKSUM(NEWID())) % 20),
    (SELECT Name FROM @LastNames WHERE Idx = ABS(CHECKSUM(NEWID())) % 20),
    N'vevo' + CAST(n.N AS VARCHAR(10)) + N'@example.com',
    CASE WHEN ABS(CHECKSUM(NEWID())) % 10 = 0 THEN NULL
         ELSE N'+36 ' + CAST(20 + ABS(CHECKSUM(NEWID())) % 51 AS VARCHAR(2))
              + N' ' + CAST(1000000 + ABS(CHECKSUM(NEWID())) % 8999999 AS VARCHAR(7))
    END,
    n.N
FROM #Numbers n
WHERE n.N <= @Customers;

SET IDENTITY_INSERT dbo.Customer OFF;

-------------------------------------------------------------------
-- 7) Product
-------------------------------------------------------------------
DECLARE @Prices TABLE (ProductId INT PRIMARY KEY, Price DECIMAL(18,2));

INSERT INTO @Prices (ProductId, Price)
SELECT n.N, CAST(ROUND(1 + RAND(CHECKSUM(NEWID())) * 999, 2) AS DECIMAL(18,2))
FROM #Numbers n
WHERE n.N <= @Products;

SET IDENTITY_INSERT dbo.Product ON;

INSERT INTO dbo.Product (Id, CreatedAt, Name, Description, CategoryId, SupplierId, Price, StockQuantity, IsActive)
SELECT
    n.N,
    DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 1095), SYSUTCDATETIME()),
    (SELECT Name FROM @ProductAdjectives WHERE Idx = ABS(CHECKSUM(NEWID())) % 10)
        + N' ' + (SELECT Name FROM @ProductNouns WHERE Idx = ABS(CHECKSUM(NEWID())) % 15)
        + N' #' + RIGHT('00000' + CAST(n.N AS VARCHAR(5)), 5),
    CASE WHEN ABS(CHECKSUM(NEWID())) % 10 = 0 THEN NULL
         ELSE N'Részletes leírás a termékhez. Cikkszám: PRD-' + RIGHT('000000' + CAST(n.N AS VARCHAR(6)), 6) + N'.'
    END,
    1 + ABS(CHECKSUM(NEWID())) % @Categories,
    1 + ABS(CHECKSUM(NEWID())) % @Suppliers,
    p.Price,
    ABS(CHECKSUM(NEWID())) % 500,
    CASE WHEN ABS(CHECKSUM(NEWID())) % 10 <> 0 THEN 1 ELSE 0 END
FROM #Numbers n
JOIN @Prices p ON p.ProductId = n.N
WHERE n.N <= @Products;

SET IDENTITY_INSERT dbo.Product OFF;

-------------------------------------------------------------------
-- 8) Order + OrderItem
-------------------------------------------------------------------
CREATE TABLE #OrderItemsStaging
(
    Id        BIGINT,
    OrderId   INT,
    ProductId INT,
    Quantity  INT,
    UnitPrice DECIMAL(18,2)
);

DECLARE @OrderId    INT = 1;
DECLARE @ItemId     BIGINT = 0;
DECLARE @OrderDate  DATETIME;
DECLARE @CustomerId INT;
DECLARE @StatusId   INT;
DECLARE @Total      DECIMAL(18,2);
DECLARE @ItemCount  INT;
DECLARE @j          INT;
DECLARE @ProductId  INT;
DECLARE @Quantity   INT;
DECLARE @ListPrice  DECIMAL(18,2);
DECLARE @UnitPrice  DECIMAL(18,2);

SET IDENTITY_INSERT dbo.[Order] ON;

WHILE @OrderId <= @Orders
BEGIN
    SET @OrderDate  = DATEADD(MINUTE, -(ABS(CHECKSUM(NEWID())) % 1051200), SYSUTCDATETIME()); -- 2*365*24*60 perc
    SET @CustomerId = 1 + ABS(CHECKSUM(NEWID())) % @Customers;
    SET @StatusId   = 1 + ABS(CHECKSUM(NEWID())) % 5;
    SET @ItemCount  = 1 + ABS(CHECKSUM(NEWID())) % @MaxItemsPerOrder;
    SET @Total      = 0;
    SET @j          = 0;

    WHILE @j < @ItemCount
    BEGIN
        SET @ProductId = 1 + ABS(CHECKSUM(NEWID())) % @Products;
        SET @Quantity  = 1 + ABS(CHECKSUM(NEWID())) % 10;

        SELECT @ListPrice = Price FROM @Prices WHERE ProductId = @ProductId;

        SET @UnitPrice = CAST(ROUND(@ListPrice * (0.9 + RAND(CHECKSUM(NEWID())) * 0.2), 2) AS DECIMAL(18,2));
        SET @Total += @Quantity * @UnitPrice;

        SET @ItemId += 1;
        INSERT INTO #OrderItemsStaging (Id, OrderId, ProductId, Quantity, UnitPrice)
        VALUES (@ItemId, @OrderId, @ProductId, @Quantity, @UnitPrice);

        SET @j += 1;
    END;

    INSERT INTO dbo.[Order] (Id, OrderDate, CustomerId, StatusId, TotalAmount)
    VALUES (@OrderId, @OrderDate, @CustomerId, @StatusId, @Total);

    SET @OrderId += 1;
END;

SET IDENTITY_INSERT dbo.[Order] OFF;

SET IDENTITY_INSERT dbo.OrderItem ON;

INSERT INTO dbo.OrderItem (Id, OrderId, ProductId, Quantity, UnitPrice)
SELECT Id, OrderId, ProductId, Quantity, UnitPrice
FROM #OrderItemsStaging;

SET IDENTITY_INSERT dbo.OrderItem OFF;

DROP TABLE #OrderItemsStaging;

-------------------------------------------------------------------
-- 9) Identity-k a tényleges maximumra állítása
-------------------------------------------------------------------
DBCC CHECKIDENT ('dbo.Category',  RESEED);
DBCC CHECKIDENT ('dbo.Supplier',  RESEED);
DBCC CHECKIDENT ('dbo.Addressee', RESEED);
DBCC CHECKIDENT ('dbo.Customer',  RESEED);
DBCC CHECKIDENT ('dbo.Product',   RESEED);
DBCC CHECKIDENT ('dbo.[Order]',   RESEED);
DBCC CHECKIDENT ('dbo.OrderItem', RESEED);

DROP TABLE #Numbers;

PRINT N'Seed kész.';

END