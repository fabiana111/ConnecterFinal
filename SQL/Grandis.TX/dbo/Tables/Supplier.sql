CREATE TABLE [dbo].[Supplier]
(
	[Id]                BIGINT          IDENTITY (1, 1) NOT NULL,
	[LastName]          NVARCHAR (100)                  NOT NULL,
    [Email]             NVARCHAR (100)                  NOT NULL,
    [PhoneNumber]       VARCHAR (50)                    NOT NULL,
	[IsActive]			BIT	   DEFAULT 1				NOT NULL,

	CONSTRAINT [PK_Supplier] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE)
)
