CREATE TABLE [dbo].[Cashflow] (
    [Id]             INT                IDENTITY (1, 1) NOT NULL,
    [CashAccountId]  INT                NOT NULL,
    [TradeId]        INT                NULL,
    [CashflowDate]   DATETIMEOFFSET (7) NOT NULL,
    [Amount]         DECIMAL (18, 2)    NOT NULL,
    [CurrencyCode]   CHAR (3)           NOT NULL,
    [CashflowType]   NVARCHAR (50)      NULL,
    [Description]    NVARCHAR (255)     NULL,
    [CreatedAt]      DATETIME2 (7)      DEFAULT (sysdatetime()) NULL,
    [ModifiedAt]     DATETIMEOFFSET (7) NULL,
    [SettlementDate] DATETIMEOFFSET (7) NULL,
    [CreatedDate]    DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedDate]   DATETIMEOFFSET (7) NULL,
    [CreatedBy]      INT                NULL,
    [ModifiedBy]     INT                NULL,
    [FundId]         INT                NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Cashflow_TradeId_NotNull_For_TradeType] CHECK (NOT ([CashflowType]='Interest' OR [CashflowType]='Fee' OR [CashflowType]='Transfer') OR [TradeId] IS NULL),
    FOREIGN KEY ([CashAccountId]) REFERENCES [dbo].[CashAccount] ([Id]),
    FOREIGN KEY ([TradeId]) REFERENCES [dbo].[RepoTrades] ([Id]),
    CONSTRAINT [FK_Cashflow_Fund] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Fund] ([Id]),
    CONSTRAINT [FK_Cashflows_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_Cashflows_ModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[User] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Cashflow_FundId]
    ON [dbo].[Cashflow]([FundId] ASC);


GO

        CREATE TRIGGER dbo.trg_Cashflow_SetFundId
        ON dbo.Cashflow
        AFTER INSERT
        AS
        BEGIN
            SET NOCOUNT ON;
            UPDATE cf
            SET cf.FundId = ca.FundId
            FROM dbo.Cashflow cf
            JOIN inserted i ON cf.Id = i.Id
            LEFT JOIN dbo.CashAccount ca ON cf.CashAccountId = ca.Id
            WHERE cf.FundId IS NULL;
        END
        