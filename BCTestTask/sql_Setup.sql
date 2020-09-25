USE [master]
GO

CREATE DATABASE [BCTestTask]
GO

USE [BCTestTask]
GO

CREATE TABLE [dbo].[TestTable] (
	[id] [int] IDENTITY(1, 1) NOT NULL
	,[Value] [bigint] NULL
	,[Description] [varchar](255) NULL
	,CONSTRAINT [PK_TestTable] PRIMARY KEY CLUSTERED ([id] ASC)
	)
GO

EXEC sys.sp_cdc_enable_db
GO

EXEC sys.sp_cdc_enable_table @source_schema = N'dbo'
	,@source_name = N'TestTable'
	,@role_name = NULL
	,@supports_net_changes = 1
GO

;

INSERT INTO [dbo].[TestTable] (
	Value
	,Description
	)
SELECT CHECKSUM(id, schema_ver) [Value]
	,CONCAT (
		'Description '
		,CHECKSUM(id, schema_ver)
		)
FROM sysobjects;

WITH Upd
AS (
	SELECT TOP 10 *
	FROM [dbo].[TestTable]
	)
UPDATE Upd
SET [Value] = [Value] + 1;

WITH Del
AS (
	SELECT TOP 10 *
	FROM [dbo].[TestTable]
	ORDER BY Description DESC
	)
DELETE
FROM Del
