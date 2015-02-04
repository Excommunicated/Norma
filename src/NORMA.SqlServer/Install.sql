DECLARE @TARGET_SCHEMA_VERSION INT;
SET @TARGET_SCHEMA_VERSION = 1;

PRINT 'Installing Norma SQL objects...';

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;

-- Create the database schema if it doesn't exists
IF NOT EXISTS (SELECT [schema_id] FROM [sys].[schemas] WHERE [name] = 'Norma')
BEGIN
    EXEC (N'CREATE SCHEMA [Norma]');
    PRINT 'Created database schema [Norma]';
END
ELSE
    PRINT 'Database schema [Norma] already exists';
    
DECLARE @SCHEMA_ID int;
SELECT @SCHEMA_ID = [schema_id] FROM [sys].[schemas] WHERE [name] = 'Norma';

-- Create the Norma.Schema table if not exists
IF NOT EXISTS(SELECT [object_id] FROM [sys].[tables] 
    WHERE [name] = 'Schema' AND [schema_id] = @SCHEMA_ID)
BEGIN
    CREATE TABLE [Norma].[Schema](
        [Version] [int] NOT NULL,
        CONSTRAINT [PK_NORMA_Schema] PRIMARY KEY CLUSTERED ([Version] ASC)
    );
    PRINT 'Created table [Norma].[Schema]';
END
ELSE
    PRINT 'Table [Norma].[Schema] already exists';
    
DECLARE @CURRENT_SCHEMA_VERSION int;
SELECT @CURRENT_SCHEMA_VERSION = [Version] FROM [Norma].[Schema];

PRINT 'Current Norma schema version: ' + CASE @CURRENT_SCHEMA_VERSION WHEN NULL THEN 'none' ELSE CONVERT(nvarchar, @CURRENT_SCHEMA_VERSION) END;

IF @CURRENT_SCHEMA_VERSION IS NOT NULL AND @CURRENT_SCHEMA_VERSION > @TARGET_SCHEMA_VERSION
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR(N'Norma current database schema version %d is newer than the configured SqlServerStorage schema version %d. Please update to the latest Norma.SqlServer NuGet package.', 11, 1,
        @CURRENT_SCHEMA_VERSION, @TARGET_SCHEMA_VERSION);
END
ELSE
BEGIN
	IF @CURRENT_SCHEMA_VERSION IS NULL
	BEGIN
		PRINT 'Installing schema version 1';

		--Create the AuditLog Table
		CREATE TABLE [Norma].[AuditLog] (
		[Id]              UNIQUEIDENTIFIER NOT NULL,
		[Created]         DATETIME         NOT NULL,
		[LastUpdated]	  DATETIME		   NOT NULL,
		[LastUpdatedUser] NVARCHAR (MAX)   NOT NULL,
		[EntityFullName]  NVARCHAR (MAX)   NULL,
		[EntityId]        NVARCHAR (MAX)   NULL,
		CONSTRAINT [PK_Norma_AuditLog] PRIMARY KEY CLUSTERED ([Id] ASC)
		);

		--Create the AuditLogChange Table
		CREATE TABLE [Norma].[AuditLogChange] (
		[Id]			 UNIQUEIDENTIFIER NOT NULL,
		[AuditLogId]	 UNIQUEIDENTIFIER NOT NULL,
		[Created]		 DATETIME		  NOT NULL,
		[User]           NVARCHAR (MAX)   NULL,
		[OldValue]       NVARCHAR (MAX)   NULL,
		[NewValue]       NVARCHAR (MAX)   NULL,
		[PropertyName]   NVARCHAR (MAX)   NULL,
		[Operation]      INT              NOT NULL,
		CONSTRAINT [PK_Norma_AuditLogChange] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_Norma_AuditLog] FOREIGN KEY ([AuditLogId]) REFERENCES [Norma].[AuditLog] ([Id])
		);

		SET @CURRENT_SCHEMA_VERSION = 1;
	END
	UPDATE [Norma].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
	IF @@ROWCOUNT = 0 
		INSERT INTO [Norma].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)        

    PRINT CHAR(13) + 'Norma database schema installed';

    COMMIT TRANSACTION;
    PRINT 'Norma SQL objects installed';
END