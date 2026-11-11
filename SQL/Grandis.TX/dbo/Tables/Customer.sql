CREATE TABLE [dbo].[Customer]
(
	[Id]                BIGINT          IDENTITY (1, 1) NOT NULL,
    [CreditedAt]        DATETIME DEFAULT (getutcdate()) NOT NULL,
    [FirstName]         NVARCHAR (50)                   NOT NULL,
    [LastName]          NVARCHAR (50)                   NOT NULL,
    [Email]             NVARCHAR (100)                  NOT NULL,
    [PhoneNumber]       VARCHAR (25)                        NULL,
    [AddresseeId]		BIGINT							NOT NULL,

    CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE),
    CONSTRAINT [FK_Customer_Addressee_AddresseeId] FOREIGN KEY ([AddresseeId]) REFERENCES [dbo].[Addressee] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_Customer_AddresseeId] ON [dbo].[Customer] ([AddresseeId]);

GO
CREATE NONCLUSTERED INDEX [IX_Customer_Email] ON [dbo].[Customer] ([Email]);
