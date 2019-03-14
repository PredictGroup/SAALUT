echo off
ECHO Отправка файла названия магазина...
copy DATA\sm%1f61.dat  > NUL
digiwtcp WR 61 %1
del sm%1f61.dat

echo. >> ERR_SEND\shopname
time /t >> ERR_SEND\shopname
copy ERR_SEND\shopname+result ERR_SEND\shopname > NUL
