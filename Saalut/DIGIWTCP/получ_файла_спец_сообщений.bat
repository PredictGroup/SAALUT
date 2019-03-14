echo off

ECHO Получение файла спец. сообщений...
digiwtcp RD 59 %1 
remove_e2 sm%1f59.dat DATA\sm%1f59.dat > NUL
del sm%1f59.dat  

echo. >> ERR_GET\specmessage
time /t >> ERR_GET\specmessage
copy ERR_GET\specmessage+result ERR_GET\specmessage > NUL
