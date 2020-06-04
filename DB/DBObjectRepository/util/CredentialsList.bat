::list credentials for other user. Doesn't seem to work.
runas /user:<serviceAccountUsername> "%windir%\system32\cmdkey.exe /list"
pause
