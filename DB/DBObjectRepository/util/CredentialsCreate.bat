::create credentials for other user.
runas /user:<serviceAccountUsername> "%windir%\system32\cmdkey.exe /generic:LegacyGeneric:target=git:https://<gitUsername>@<gitURl> /user:<gitUsername> /pass:<gitPassword>"
runas /user:<serviceAccountUsername> "%windir%\system32\cmdkey.exe /generic:LegacyGeneric:target=git:https://<gitURl> /user:<gitUsername> /pass:<gitPassword>"
pause
