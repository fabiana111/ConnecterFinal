CREATE TABLE [dbo].[Category]
(
	[Id]                BIGINT          IDENTITY (1, 1)	 NOT NULL,
	[Name]				NVARCHAR (100)                   NOT NULL,
	[Description]       NVARCHAR (250)                   NOT NULL,
	[IsActive]			BIT			DEFAULT 1			 NOT NULL,

	CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE)
)
