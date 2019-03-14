echo off
ECHO Отправка файла места производства...
copy DATA\sm%1f57.dat  > NUL
digiwtcp WR 57 %1
del sm%1f57.dat

echo. >> ERR_SEND\place
time /t >> ERR_SEND\place
copy ERR_SEND\place+result ERR_SEND\place > NUL