echo off
ECHO Отправка файла PLU...
copy DATA\sm%1f37.dat  > NUL
digiwtcp WR 37 %1
del sm%1f37.dat

echo. >> ERR_SEND\PLU
time /t >> ERR_SEND\PLU
copy ERR_SEND\PLU+result ERR_SEND\PLU > NUL

