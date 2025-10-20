CREATE TABLE [dbo].[CashAccount] (
    [Id]           INT                IDENTITY (1, 1) NOT NULL,
    [AccountName]  NVARCHAR (100)     NULL,
    [CurrencyCode] CHAR (3)           NOT NULL,
    [OwnerType]    NVARCHAR (50)      NULL,
    [OwnerId]      INT                NOT NULL,
    [Balance]      DECIMAL (18, 2)    DEFAULT ((0.0)) NULL,
    [CreatedAt]    DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedAt]   DATETIMEOFFSET (7) NULL,
    [FundId]       INT                NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CashAccount_Fund] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Fund] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_CashAccount_FundId]
    ON [dbo].[CashAccount]([FundId] ASC);

