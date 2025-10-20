CREATE TABLE [dbo].[RepoTrades] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [TradeDate]        DATETIME2 (7)      CONSTRAINT [DF_RepoTrades_TradeDate] DEFAULT (sysdatetimeoffset()) NOT NULL,
    [SecurityId]       BIGINT             NOT NULL,
    [Notional]         DECIMAL (18, 2)    NOT NULL,
    [Rate]             DECIMAL (8, 4)     NOT NULL,
    [MaturityDate]     DATETIME2 (7)      NOT NULL,
    [CounterpartyId]   INT                NULL,
    [CreatedAt]        DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedAt]       DATETIMEOFFSET (7) NULL,
    [StartDate]        DATETIMEOFFSET (7) NULL,
    [Direction]        VARCHAR (10)       NULL,
    [CollateralTypeId] SMALLINT           NULL,
    [CreatedDate]      DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedDate]     DATETIMEOFFSET (7) NULL,
    [CreatedBy]        INT                NULL,
    [ModifiedBy]       INT                NULL,
    [Status]           VARCHAR (10)       NULL,
    [FundId]           INT                NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_RepoTrades_Direction] CHECK ([Direction]='Lend' OR [Direction]='Borrow'),
    CONSTRAINT [CK_RepoTrades_Notional_Positive] CHECK ([Notional]>(0)),
    CONSTRAINT [CK_RepoTrades_Rate_NonNegative] CHECK ([Rate]>=(0)),
    CONSTRAINT [FK_RepoTrades_CollateralType] FOREIGN KEY ([CollateralTypeId]) REFERENCES [dbo].[CollateralTypes] ([Id]),
    CONSTRAINT [FK_RepoTrades_Counterparty] FOREIGN KEY ([CounterpartyId]) REFERENCES [dbo].[Counterparty] ([Id]),
    CONSTRAINT [FK_RepoTrades_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_RepoTrades_Fund] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Fund] ([Id]),
    CONSTRAINT [FK_RepoTrades_ModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_RepoTrades_SecID] FOREIGN KEY ([SecurityId]) REFERENCES [dbo].[Security] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_RepoTrades_FundId]
    ON [dbo].[RepoTrades]([FundId] ASC);

