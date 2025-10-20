CREATE TABLE [dbo].[CollateralTypes] (
    [Id]             SMALLINT           IDENTITY (1, 1) NOT NULL,
    [AssetType]      VARCHAR (50)       NOT NULL,
    [CollateralType] VARCHAR (50)       NULL,
    [CreatedAt]      DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedAt]     DATETIMEOFFSET (7) NULL,
    [CreatedDate]    DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedDate]   DATETIMEOFFSET (7) NULL,
    [CreatedBy]      INT                NULL,
    [ModifiedBy]     INT                NULL,
    CONSTRAINT [PK__Collater__7F6321AB1EF33949] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CollateralTypes_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_CollateralTypes_ModifiedBy] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[User] ([Id])
);

