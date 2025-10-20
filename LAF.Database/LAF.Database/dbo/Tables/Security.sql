CREATE TABLE [dbo].[Security] (
    [Id]           BIGINT             IDENTITY (1, 1) NOT NULL,
    [ISIN]         VARCHAR (20)       NOT NULL,
    [Description]  NVARCHAR (255)     NOT NULL,
    [AssetType]    VARCHAR (50)       NOT NULL,
    [Issuer]       NVARCHAR (100)     NOT NULL,
    [Currency]     CHAR (3)           NOT NULL,
    [CreatedAt]    DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedAt]   DATETIMEOFFSET (7) NULL,
    [MaturityDate] DATETIMEOFFSET (7) NULL,
    [Coupon]       DECIMAL (9, 4)     NULL,
    [IssuerType]   VARCHAR (50)       NULL,
    [CreatedBy]    INT                NULL,
    [ModifiedBy]   INT                NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Securities_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_Securities_ModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [UQ_Security_ISIN] UNIQUE NONCLUSTERED ([ISIN] ASC)
);

