CREATE TABLE [dbo].[Addressee]
(
	[Id]                   BIGINT            IDENTITY (1, 1) NOT NULL,
    [AddresseeCountry]     NVARCHAR (2)                      NOT NULL,
    [AddresseeZip]         NVARCHAR (12)                     NOT NULL,
    [AddresseeAddress]     NVARCHAR (100)                    NOT NULL,
    [AddresseeCity]        NVARCHAR (25)                     NOT NULL,

    CONSTRAINT [PK_Addressees] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION = PAGE)
)
