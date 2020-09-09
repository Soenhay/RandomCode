## Setup:

- Install git
- git setup commands:
	- git config --global user.name "<gitUsername>"
	- git config --global user.email "<gitUserEmail>"
	- git config http.sslVerify false
- .gitconfig file is located at C:\Users\<username>\. It should also be located at C:\Users\<serviceAccountUsername>\ for the scheduled task to work. Just copy it over.
- .gitconfig file looks like this:
[user]
	name = <gitUsername>
	email = <gitUserEmail>
[http]
	sslVerify = false

- Clone repo to folder on server.
	- navigate to E:\backup\
	- open cmd prompt.
	- git clone  https://<gitUrl>
	- should clone files into new subdirectory named after the project.
- Change DoBackup.bat to use the correct sql server.
- Make sure the application user ( service account user ) has read/write permissions on the directory.
- Create scheduled task to run "Run.bat" nightly.
	- The scheduled task should run as the application user ( service account ) which also has access to the sql server and permissions to view definitions and execute.
	- The scheduled task should run whether the user is logged on or not and with highest privileges. 
	- The scheduled task should run as hidden.
	- The scheduled task action should run program "Run.bat" with parameters "> output.txt 2> output.err".


## Files

 - Run.bat: main entry point for scheduled task.
 
 - DoBackup.bat: Run sql to get object definitions and output to files.

 - GetParameters.sql: Create parameters.csv, a list of dbNames, objectNames and output filenames.
 
 - GetObjectInfo.sql: Get info for a single object.
 
 - output.txt, output.err. Files created by scheduled task to help with troubleshooting. Scheduled task needs the following params for these files to be generated: "> output.txt 2> output.err".
 
 - test folder: output for testCmd.bat.
 
 - testCmd.bat: Test calling GetObjectInfo.sql for one object. Outputs to test folder.
 
 - util folder: Various helper files.
 
 - util/CrediantialsCreate.bat: used to create git credentials for service account user.
 
 - util/CredentialsList.bat: used to list credentials for service account user. Doesn't currently work.
 
 - util/GetObjectDefinitions.sql: query to list all object definitions. Not used in main process.
 
 - util/testRunAs.bat: test runas different user. Was used to test code for credentialsCreate. Might not work in utils folder.
 
 

## Troubleshooting:

- Use testCmd.bat to test out running sqlcmd on one object.

- ‘sqlcmd’is not recognized as an internal or external command.
   - Install the latest sqlcmd. Search online for "Download the latest version of sqlcmd Utility".
   - You might get a popup saying this is also required (if so then install it first): Microsoft ODBC Driver 17 for SQL Server
   
- Make sure newline is at the end of each git command in the bat file.

- List credentials and use that info to add them to the user with runas (see CredentialsCreate.bat):

E:\backup\databasebackup>cmdkey /list

Currently stored credentials:

    Target: LegacyGeneric:target=git:https://<gitUsername>@<gitUrl>
    Type: Generic
    User: <gitUsername>
    Local machine persistence

    Target: LegacyGeneric:target=git:https://<gitUrl>
    Type: Generic
    User: <gitUsername>
    Local machine persistence
	
- Search in windows for "Manage Credentials" to see what credentials you have.
