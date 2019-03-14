echo off
echo Отправка логотипов...
copy DATA\sm%1f54.dat  > NUL
digiwtcp WR 54 %1
del sm%1f54.dat

echo. >> ERR_SEND\logo
time /t >> ERR_SEND\logo
copy ERR_SEND\logo+result ERR_SEND\logo > NUL