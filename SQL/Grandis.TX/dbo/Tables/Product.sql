CREATE TABLE [dbo].[Product]
(
	[Id]                BIGINT          IDENTITY (1, 1) NOT NULL,
	[CreatedAt]         DATETIME DEFAULT (getutcdate()) NOT NULL,
	[Name]				NVARCHAR (100)                  NOT NULL,
	[Description]       NVARCHAR (MAX)						NULL,
	[CategoryId]		BIGINT							NOT NULL,
	[SupplierId]		BIGINT							NOT NULL,
	[Price]				DECIMAL (18, 2)					NOT NULL,
	[StockQuantity]     INT								NOT NULL,
	[IsActive]			BIT			DEFAULT 1			NOT NULL,

	CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE),
	CONSTRAINT [FK_Product_Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id]),
	CONSTRAINT [FK_Product_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Supplier] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_Product_CategoryId] ON [dbo].[Product] ([CategoryId]);

GO
CREATE NONCLUSTERED INDEX [IX_Product_SupplierId] ON [dbo].[Product] ([SupplierId]);

GO
CREATE NONCLUSTERED INDEX [IX_Product_IsActive_Price] ON [dbo].[Product] ([IsActive], [Price]) 
INCLUDE ([Name], [StockQuantity]);
