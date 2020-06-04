SET NOCOUNT ON

DECLARE @DB_Name VARCHAR(100);
DECLARE @Command NVARCHAR(MAX); 
DECLARE @ServerPathName VARCHAR(100) =  REPLACE(REPLACE(REPLACE(@@SERVERNAME, ' ', '_'), '\', '_'), '.', '_');
DECLARE @OneResult VARCHAR(1000);

DECLARE @ObjectInfo TABLE
( 
  DatabaseName VARCHAR(1000),
  QualifiedObjectName VARCHAR(1000),
  OutputFilePath VARCHAR(1000)
)

DECLARE @Errors TABLE
(
	ServerName     NVARCHAR(128)
   , DatabaseName   NVARCHAR(128)
   , ErrorNumber    INT
   , ErrorSeverity  INT
   , ErrorState     INT
   , ErrorProcedure NVARCHAR(1000)
   , ErrorLine      INT
   , ErrorMessage   NVARCHAR(1000)
);

DECLARE database_cursor CURSOR
FOR SELECT name
    FROM   MASTER.sys.sysdatabases
WHERE name NOT IN ('master','model','msdb','tempdb')  -- exclude these databases
AND name NOT IN ('ListExcludedDBsHere')

OPEN database_cursor; 

FETCH NEXT FROM database_cursor INTO @DB_Name; 

WHILE @@Fetch_Status = 0
BEGIN
    SELECT @Command = '
    USE ['+@DB_Name+']
    SELECT  
     DatabaseName
    , DatabaseName + ''.'' + SchemaName + ''.'' + ObjectName AS QualifiedObjectName
    , ServerName+''\''+DatabaseName+''\''+SchemaName+''\''+ObjectName+''.sql'' AS OutputFilePath
    FROM
    (
	   SELECT '''+@ServerPathName+''' AS ServerName
		   , DB_NAME() AS DatabaseName
		   , ss.name AS SchemaName
		   , so.name AS ObjectName
		   , so.type
		   , so.type_desc
	   FROM   SYS.objects AS so
			JOIN SYS.schemas AS ss ON ss.schema_id = so.schema_id
	   WHERE  so.TYPE IN(''FN'', ''IF'', ''P'', ''TF'')
    ) AS a;
    ';
    --PRINT @Command;
    BEGIN TRY
	   INSERT  INTO @ObjectInfo
	   EXEC sp_executesql @Command;
    END TRY
    BEGIN CATCH
	   INSERT INTO @Errors
	   SELECT @ServerPathName AS ServerPathName
			 , @DB_Name AS DbName
			 ,ERROR_NUMBER() AS ErrorNumber
			 ,ERROR_SEVERITY() AS ErrorSeverity
			 ,ERROR_STATE() AS ErrorState
			 ,ERROR_PROCEDURE() AS ErrorProcedure
			 ,ERROR_LINE() AS ErrorLine
			 ,ERROR_MESSAGE() AS ErrorMessage;
    END CATCH

    FETCH NEXT FROM database_cursor INTO @DB_Name;
END; 

CLOSE database_cursor; 
DEALLOCATE database_cursor; 

--For trouble shooting
--SELECT QualifiedObjectName + ',' + OutputFilePath FROM @ObjectInfo
--ORDER BY QualifiedObjectName
--SELECT * FROM @Errors

DECLARE results_cursor CURSOR
FOR SELECT DatabaseName + ',' + QualifiedObjectName + ',' + OutputFilePath
    FROM   @ObjectInfo
ORDER BY QualifiedObjectName

OPEN results_cursor; 

FETCH NEXT FROM results_cursor INTO @OneResult; 

WHILE @@Fetch_Status = 0
BEGIN
    
    PRINT @OneResult
    FETCH NEXT FROM results_cursor INTO @OneResult;
END; 

CLOSE results_cursor; 
DEALLOCATE results_cursor; 



/*

    SET NOCOUNT ON

DECLARE @OutPutDirectory VARCHAR(1000);
DECLARE @DB_Name VARCHAR(100);
DECLARE @Command NVARCHAR(MAX); 
DECLARE @ServerPathName VARCHAR(100);


SELECT @OutPutDirectory = '';

SELECT @OutPutDirectory = CASE WHEN @OutPutDirectory like '%\' THEN SUBSTRING(@OutPutDirectory, 0, LEN(@OutPutDirectory) - 1) ELSE @OutPutDirectory END

SELECT @ServerPathName =  REPLACE(REPLACE(@@SERVERNAME, ' ', '_'), '\', '_') 

DECLARE @ObjectInfo TABLE
( 
  ServerName  NVARCHAR(128),
  DatabaseName  NVARCHAR(128),
  SchemaName  NVARCHAR(128),
  ObjectName  NVARCHAR(128),
  type char(2),
  type_desc nvarchar(60),
  FilePath VARCHAR(1000),
  ObjectDefinition NVARCHAR(MAX)
)

DECLARE @Errors TABLE
(
	ServerName     NVARCHAR(128)
   , DatabaseName   NVARCHAR(128)
   , ErrorNumber    INT
   , ErrorSeverity  INT
   , ErrorState     INT
   , ErrorProcedure NVARCHAR(1000)
   , ErrorLine      INT
   , ErrorMessage   NVARCHAR(1000)
);

DECLARE database_cursor CURSOR
FOR SELECT name
    FROM   MASTER.sys.sysdatabases
WHERE name NOT IN ('master','model','msdb','tempdb')  -- exclude these databases

OPEN database_cursor; 

FETCH NEXT FROM database_cursor INTO @DB_Name; 

WHILE @@Fetch_Status = 0
BEGIN
    SELECT @Command = '
    USE ['+@DB_Name+']
    SELECT ServerName
		 , DatabaseName
		 , SchemaName
		 , ObjectName
		 , type
		 , type_desc
		 , ''' + @OutPutDirectory +  ''' + ''\'' + ServerName + ''\'' + DatabaseName + ''\'' + SchemaName + ''\'' + ObjectName + ''.sql'' AS FilePath
		 , ObjectDefinition
	FROM(
		 SELECT   '''+@ServerPathName+''' AS ServerName
			  , '''+@DB_Name+''' AS DatabaseName  
			  , ss.name AS SchemaName
			  , so.name AS ObjectName
			  , so.type
			  , so.type_desc
			  , ISNULL(OBJECT_DEFINITION(OBJECT_ID(ss.name + ''.'' + so.name)), '''') AS ObjectDefinition
		  FROM   SYS.objects so
				JOIN SYS.schemas ss ON ss.schema_id = so.schema_id
		  WHERE  so.TYPE IN(''FN'', ''IF'', ''P'', ''TF'')
	)a
    ';
    --PRINT @Command;
    BEGIN TRY
	   INSERT  INTO @ObjectInfo
	   EXEC sp_executesql @Command;
    END TRY
    BEGIN CATCH
	   INSERT INTO @Errors
	   SELECT @ServerPathName AS ServerPathName
			 , @DB_Name AS DbName
			 ,ERROR_NUMBER() AS ErrorNumber
			 ,ERROR_SEVERITY() AS ErrorSeverity
			 ,ERROR_STATE() AS ErrorState
			 ,ERROR_PROCEDURE() AS ErrorProcedure
			 ,ERROR_LINE() AS ErrorLine
			 ,ERROR_MESSAGE() AS ErrorMessage;
    END CATCH

    FETCH NEXT FROM database_cursor INTO @DB_Name;
END; 

CLOSE database_cursor; 
DEALLOCATE database_cursor; 

SELECT * FROM @ObjectInfo 
ORDER BY DatabaseName, SchemaName, ObjectName

--For trouble shooting
--SELECT * FROM @Errors


*/