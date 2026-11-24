-- Event Management System: database schema (SQL Server)
-- Creates the Events database and its tables. Safe to run more than once.

IF DB_ID(N'Events') IS NULL
    CREATE DATABASE [Events];
GO

USE [Events];
GO

IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
CREATE TABLE [dbo].[Events] (
    [ID]               INT            IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (100) NOT NULL,
    [StartDate]        DATETIME       NOT NULL,
    [EndDate]          DATETIME       NOT NULL,
    [MaxRegistrations] INT            NOT NULL,
    [Location]         NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE [dbo].[Users] (
    [ID]          INT           IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (50) NOT NULL,
    [DateOfBirth] DATE          NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Many-to-many link between events and users (one row per registration)
IF OBJECT_ID(N'dbo.EventUser', N'U') IS NULL
CREATE TABLE [dbo].[EventUser] (
    [EventRef] INT      NOT NULL,
    [UserRef]  INT      NOT NULL,
    [Creation] DATETIME NOT NULL CONSTRAINT [DF_EventUser_Creation] DEFAULT (GETDATE()),
    CONSTRAINT [PK_EventUser] PRIMARY KEY CLUSTERED ([EventRef] ASC, [UserRef] ASC),
    CONSTRAINT [FK_EventUser_Events] FOREIGN KEY ([EventRef]) REFERENCES [dbo].[Events] ([ID]),
    CONSTRAINT [FK_EventUser_Users] FOREIGN KEY ([UserRef]) REFERENCES [dbo].[Users] ([ID])
);
GO
