CREATE TABLE [dbo].[MeetingExtra]
(
[MeetingId] [int] NOT NULL,
[Field] [nvarchar] (50) NOT NULL,
[Data] [nvarchar] (max) NULL,
[DataType] [nvarchar] (5) NULL,
[StrValue] [nvarchar] (200) NULL,
[DateValue] [datetime] NULL,
[IntValue] [int] NULL,
[BitValue] [bit] NULL,
[Type] AS ((((case when [StrValue] IS NOT NULL then 'Code' else '' end+case when [Data] IS NOT NULL then 'Text' else '' end)+case when [DateValue] IS NOT NULL then 'Date' else '' end)+case when [IntValue] IS NOT NULL then 'Int' else '' end)+case when [BitValue] IS NOT NULL then 'Bit' else '' end),
[TransactionTime] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
