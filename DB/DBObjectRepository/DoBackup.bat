::Run sql scripts that generate backup scripts of some data
::   sqlcmd -S myServer\instanceName -i C:\myScript.sql -o C:\OutputFile.txt
:: https://social.technet.microsoft.com/wiki/contents/articles/35120.sql-script-passing-parameters-to-sql-script-with-batch-file-and-powershell.aspx

SETLOCAL ENABLEDELAYEDEXPANSION

sqlcmd -S <sqlServerName> -i %~dp0\GetParameters.sql -o %~dp0\parameters.csv

for /f "usebackq tokens=1,2,3 delims=," %%a in ("parameters.csv") do (
	for %%F in ("%~dp0%%c") do ( 
	if not exist "%%~dpF" MD "%%~dpF"
	)
	sqlcmd -m 1 -E -h -1 -S <sqlServerName> -i %~dp0\GetObjectInfo.sql -v dbName=%%a objectName='%%b' -o %~dp0%%c
)

::pause