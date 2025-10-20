CREATE TABLE [dbo].[Fund] (
    [Id]           INT                IDENTITY (1, 1) NOT NULL,
    [FundCode]     NVARCHAR (50)      NOT NULL,
    [FundName]     NVARCHAR (200)     NOT NULL,
    [CurrencyCode] CHAR (3)           NOT NULL,
    [IsActive]     BIT                CONSTRAINT [DF_Fund_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedAt]    DATETIMEOFFSET (7) CONSTRAINT [DF_Fund_CreatedAt] DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedAt]   DATETIMEOFFSET (7) NULL,
    [CreatedDate]  DATETIMEOFFSET (7) CONSTRAINT [DF_Fund_CreatedDate] DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ModifiedDate] DATETIMEOFFSET (7) NULL,
    [CreatedBy]    INT                NULL,
    [ModifiedBy]   INT                NULL,
    CONSTRAINT [PK_Fund] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Fund_FundCode]
    ON [dbo].[Fund]([FundCode] ASC);

