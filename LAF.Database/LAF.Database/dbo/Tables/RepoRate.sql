CREATE TABLE [dbo].[RepoRate] (
    [Id]               BIGINT             IDENTITY (1, 1) NOT NULL,
    [CounterpartyId]   INT                NOT NULL,
    [EffectiveDate]    DATETIME2 (7)      NOT NULL,
    [RepoRate]         DECIMAL (8, 4)     NOT NULL,
    [TargetCircle]     DECIMAL (18, 2)    NOT NULL,
    [FinalCircle]      DECIMAL (18, 2)    NOT NULL,
    [Active]           BIT                NOT NULL,
    [CollateralTypeId] SMALLINT           NOT NULL,
    [CreatedAt]        DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedAt]       DATETIMEOFFSET (7) NULL,
    [Tenor]            VARCHAR (10)       NULL,
    CONSTRAINT [PK_RepoRate] PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([CollateralTypeId]) REFERENCES [dbo].[CollateralTypes] ([Id]),
    CONSTRAINT [FK_RepoRate_Counterparty] FOREIGN KEY ([CounterpartyId]) REFERENCES [dbo].[Counterparty] ([Id])
);

