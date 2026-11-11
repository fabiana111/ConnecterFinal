CREATE TABLE [dbo].[OrderItem]
(
	[Id]                BIGINT          IDENTITY (1, 1) NOT NULL,
	[OrderId]			BIGINT							NOT NULL,
	[ProductId]			BIGINT							NOT NULL,
	[Quantity]			INT								NOT NULL,
	[UnitPrice]			DECIMAL (18,2)					NOT NULL,

	CONSTRAINT [PK_OrderItem] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE),
	CONSTRAINT [FK_OrderItem_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]),
	CONSTRAINT [FK_OrderItem_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_OrderItem_OrderId] ON [dbo].[OrderItem] ([OrderId]);

GO
CREATE NONCLUSTERED INDEX [IX_OrderItem_ProductId] ON [dbo].[OrderItem] ([ProductId]);