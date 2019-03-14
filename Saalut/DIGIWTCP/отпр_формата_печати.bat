echo off
ECHO Отправка формата печати...
copy DATA\sm%1f52.dat  > NUL
digiwtcp WR 52 %1
del sm%1f52.dat

echo. >> ERR_SEND\freeformat
time /t >> ERR_SEND\freeformat
copy ERR_SEND\freeformat+result ERR_SEND\freeformat > NUL