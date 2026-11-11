CREATE TABLE [dbo].[Order]
(
	[Id]                BIGINT          IDENTITY (1, 1) NOT NULL,
	[OrderDate]			DATETIME						NOT NULL,
	[CustomerId]		BIGINT							NOT NULL,
	[StatusId]			INT								NOT NULL,
	[TotalAmount]		DECIMAL (18, 2)					NOT NULL,

	CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE),
	CONSTRAINT [FK_Order_Customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_Order_CustomerId] ON [dbo].[Order] ([CustomerId]);

GO
CREATE NONCLUSTERED INDEX [IX_Order_OrderDate_StatusId] ON [dbo].[Order] ([OrderDate], [StatusId]);
