echo off
ECHO Отправка файла специальных сообщений....
copy DATA\sm%1f59.dat  > NUL
digiwtcp WR 59 %1
del sm%1f59.dat

echo. >> ERR_SEND\specmessage
time /t >> ERR_SEND\specmessage
copy ERR_SEND\specmessage+result ERR_SEND\specmessage > NUL