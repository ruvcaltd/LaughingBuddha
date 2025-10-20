CREATE TABLE [dbo].[Counterparty] (
    [Id]                    INT             IDENTITY (1, 1) NOT NULL,
    [CounterpartyCode]      VARCHAR (10)    NOT NULL,
    [CounterpartyName]      VARCHAR (100)   NOT NULL,
    [CounterpartyType]      VARCHAR (20)    DEFAULT ('Dealer') NOT NULL,
    [LegalEntityIdentifier] VARCHAR (20)    NULL,
    [CountryCode]           CHAR (2)        DEFAULT ('US') NOT NULL,
    [Region]                VARCHAR (50)    NULL,
    [IsActive]              BIT             DEFAULT ((1)) NOT NULL,
    [CreditRating]          VARCHAR (5)     NULL,
    [CreditLimit]           DECIMAL (15, 2) NULL,
    [CreatedDate]           DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [ModifiedDate]          DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([CounterpartyCode] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Counterparty_Active]
    ON [dbo].[Counterparty]([IsActive] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Counterparty_Code]
    ON [dbo].[Counterparty]([CounterpartyCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Counterparty_Name]
    ON [dbo].[Counterparty]([CounterpartyName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Counterparty_Type]
    ON [dbo].[Counterparty]([CounterpartyType] ASC);

