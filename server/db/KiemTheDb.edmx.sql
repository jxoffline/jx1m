
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 07/14/2023 15:08:45
-- Generated from EDMX file: D:\Resource_SELL\ALLRES_HTTPWEB\KIEMTHESDK\KIEMTHESDK\Database\KiemTheDb.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [KiemTheDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[ChatDatas]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ChatDatas];
GO
IF OBJECT_ID(N'[dbo].[Configs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Configs];
GO
IF OBJECT_ID(N'[dbo].[CsmLogins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CsmLogins];
GO
IF OBJECT_ID(N'[dbo].[GiftCodeLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GiftCodeLogs];
GO
IF OBJECT_ID(N'[dbo].[GiftCodes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GiftCodes];
GO
IF OBJECT_ID(N'[dbo].[KTCoins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KTCoins];
GO
IF OBJECT_ID(N'[dbo].[LoginTables]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LoginTables];
GO
IF OBJECT_ID(N'[dbo].[LogsTrans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LogsTrans];
GO
IF OBJECT_ID(N'[dbo].[NewsTables]', 'U') IS NOT NULL
    DROP TABLE [dbo].[NewsTables];
GO
IF OBJECT_ID(N'[dbo].[RechageLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RechageLogs];
GO
IF OBJECT_ID(N'[dbo].[ServerLists]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ServerLists];
GO
IF OBJECT_ID(N'[dbo].[ServerListsIos]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ServerListsIos];
GO
IF OBJECT_ID(N'[dbo].[TokenManagers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TokenManagers];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'ChatDatas'
CREATE TABLE [dbo].[ChatDatas] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ChatID] varchar(100)  NULL,
    [FromRoleName] varchar(50)  NULL,
    [ToRoleName] varchar(50)  NULL,
    [Channel] int  NULL,
    [ChatTime] datetime  NULL,
    [FileName] nvarchar(50)  NULL
);
GO

-- Creating table 'Configs'
CREATE TABLE [dbo].[Configs] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [StartKM] datetime  NULL,
    [EndKM] datetime  NULL,
    [NormalRate] float  NULL,
    [KMRate] float  NULL
);
GO

-- Creating table 'CsmLogins'
CREATE TABLE [dbo].[CsmLogins] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [LoginName] nvarchar(50)  NULL,
    [Password] nvarchar(50)  NULL,
    [Premission] int  NULL,
    [RegTime] datetime  NULL
);
GO

-- Creating table 'GiftCodeLogs'
CREATE TABLE [dbo].[GiftCodeLogs] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Code] nvarchar(50)  NULL,
    [ActiveRole] int  NULL,
    [ActiveTime] datetime  NULL,
    [CodeType] nvarchar(50)  NULL,
    [ServerID] int  NULL
);
GO

-- Creating table 'GiftCodes'
CREATE TABLE [dbo].[GiftCodes] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ServerID] int  NULL,
    [Code] varchar(50)  NULL,
    [Status] int  NULL,
    [ItemList] nvarchar(500)  NULL,
    [TimeCreate] datetime  NULL,
    [CodeType] varchar(50)  NULL,
    [MaxActive] int  NULL,
    [UserName] nvarchar(50)  NULL
);
GO

-- Creating table 'KTCoins'
CREATE TABLE [dbo].[KTCoins] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [UserID] int  NULL,
    [UserName] nvarchar(50)  NULL,
    [KCoin] int  NULL,
    [UpdateTime] datetime  NULL
);
GO

-- Creating table 'LoginTables'
CREATE TABLE [dbo].[LoginTables] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [LoginName] varchar(50)  NULL,
    [Password] varchar(50)  NULL,
    [Phone] varchar(50)  NULL,
    [Status] int  NULL,
    [Date] datetime  NULL,
    [ActiveRoleID] int  NULL,
    [ActiveRoleName] varchar(50)  NULL,
    [FullName] nvarchar(100)  NULL,
    [Email] nvarchar(50)  NULL,
    [TokenTimeExp] datetime  NULL,
    [AccessToken] nvarchar(50)  NULL,
    [Note] nvarchar(100)  NULL,
    [LastServerLogin] int  NULL,
    [LastLoginTime] datetime  NULL,
    [LastIPLogin] nvarchar(50)  NULL,
    [Commission] float  NULL
);
GO

-- Creating table 'LogsTrans'
CREATE TABLE [dbo].[LogsTrans] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [UserID] int  NULL,
    [RoleID] int  NULL,
    [RoleName] varchar(50)  NULL,
    [ServerID] int  NULL,
    [Value] int  NULL,
    [TimeTrans] datetime  NULL,
    [BeforeValue] int  NULL,
    [AfterValue] int  NULL
);
GO

-- Creating table 'NewsTables'
CREATE TABLE [dbo].[NewsTables] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Catagory] nvarchar(50)  NULL,
    [Title] nvarchar(200)  NULL,
    [Context] varchar(max)  NULL,
    [DateTime] datetime  NULL
);
GO

-- Creating table 'RechageLogs'
CREATE TABLE [dbo].[RechageLogs] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [UserID] int  NULL,
    [UserName] nvarchar(20)  NULL,
    [CoinValue] int  NULL,
    [BeforeCoin] int  NULL,
    [AfterCoin] int  NULL,
    [RechageDate] datetime  NULL,
    [RechageType] nvarchar(50)  NULL,
    [Pram_0] nvarchar(50)  NULL,
    [Pram_1] nvarchar(50)  NULL,
    [Pram_2] nvarchar(50)  NULL,
    [Pram_3] int  NULL,
    [Messenger] nvarchar(100)  NULL,
    [Status] int  NULL,
    [TransID] nvarchar(50)  NULL,
    [ValueRechage] int  NULL,
    [ActionBy] nvarchar(50)  NULL
);
GO

-- Creating table 'ServerLists'
CREATE TABLE [dbo].[ServerLists] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [strServerName] nvarchar(100)  NULL,
    [nServerOrder] int  NULL,
    [nServerPort] int  NULL,
    [nStatus] int  NULL,
    [strURL] nvarchar(50)  NULL,
    [nServerID] int  NULL,
    [nOnlineNum] int  NULL,
    [HttpServicePort] int  NULL,
    [strMaintainStarTime] datetime  NULL,
    [strMaintainTerminalTime] datetime  NULL,
    [strMaintainTxt] nvarchar(100)  NULL,
    [isTestServer] int  NULL
);
GO

-- Creating table 'ServerListsIos'
CREATE TABLE [dbo].[ServerListsIos] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [strServerName] nvarchar(100)  NULL,
    [nServerOrder] int  NULL,
    [nServerPort] int  NULL,
    [nStatus] int  NULL,
    [strURL] nvarchar(50)  NULL,
    [nServerID] int  NULL,
    [nOnlineNum] int  NULL,
    [HttpServicePort] int  NULL,
    [strMaintainStarTime] datetime  NULL,
    [strMaintainTerminalTime] datetime  NULL,
    [strMaintainTxt] nvarchar(100)  NULL,
    [isTestServer] int  NULL
);
GO

-- Creating table 'TokenManagers'
CREATE TABLE [dbo].[TokenManagers] (
    [id] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(100)  NULL,
    [tokencreate] nvarchar(50)  NULL,
    [time] bigint  NULL,
    [requestSendStatus] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'ChatDatas'
ALTER TABLE [dbo].[ChatDatas]
ADD CONSTRAINT [PK_ChatDatas]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Configs'
ALTER TABLE [dbo].[Configs]
ADD CONSTRAINT [PK_Configs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'CsmLogins'
ALTER TABLE [dbo].[CsmLogins]
ADD CONSTRAINT [PK_CsmLogins]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'GiftCodeLogs'
ALTER TABLE [dbo].[GiftCodeLogs]
ADD CONSTRAINT [PK_GiftCodeLogs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'GiftCodes'
ALTER TABLE [dbo].[GiftCodes]
ADD CONSTRAINT [PK_GiftCodes]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'KTCoins'
ALTER TABLE [dbo].[KTCoins]
ADD CONSTRAINT [PK_KTCoins]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'LoginTables'
ALTER TABLE [dbo].[LoginTables]
ADD CONSTRAINT [PK_LoginTables]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'LogsTrans'
ALTER TABLE [dbo].[LogsTrans]
ADD CONSTRAINT [PK_LogsTrans]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'NewsTables'
ALTER TABLE [dbo].[NewsTables]
ADD CONSTRAINT [PK_NewsTables]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'RechageLogs'
ALTER TABLE [dbo].[RechageLogs]
ADD CONSTRAINT [PK_RechageLogs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'ServerLists'
ALTER TABLE [dbo].[ServerLists]
ADD CONSTRAINT [PK_ServerLists]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'ServerListsIos'
ALTER TABLE [dbo].[ServerListsIos]
ADD CONSTRAINT [PK_ServerListsIos]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [id] in table 'TokenManagers'
ALTER TABLE [dbo].[TokenManagers]
ADD CONSTRAINT [PK_TokenManagers]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------