MD "%~dp0test"
sqlcmd -m 1 -E -h -1 -S <sqlServerName> -i %~dp0GetObjectInfo.sql -v dbName=<dbName> objectName='<objectName>' -o %~dp0test\<objectName>.sql
pause