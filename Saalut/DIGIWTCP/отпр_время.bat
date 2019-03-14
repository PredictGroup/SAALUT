echo on
ECHO Синхронизация времени..
set ddate=%date:~0,2%%date:~3,2%%date:~8,2%
set ctime=%time:~0,2%%time:~3,2%
echo.   >log.txt
digi_time.exe -i%1 -p2250 -d%ddate% -t%ctime%
echo. >> ERR_SEND\timer
copy ERR_SEND\timer+log.txt ERR_SEND\timer > NUL

