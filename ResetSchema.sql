USE [master]
GO

IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Numbers')
	CREATE DATABASE [numbers]
	 ON PRIMARY
	( NAME = N'numbers', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\numbers.mdf')
	 LOG ON 
	( NAME = N'numbers_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\numbers_log.ldf')
GO

USE [numbers]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Factors]') AND type in (N'U'))
	DROP TABLE [dbo].[Factors]
GO

CREATE TABLE Factors
(
	[Value] [int] NOT NULL,
	[Prime] [int] NOT NULL,
	[Degree] [int] NOT NULL,
	CONSTRAINT [PK_Factors] PRIMARY KEY CLUSTERED
	(
		[Value] ASC,
		[Prime] ASC
	)
)
