echo off
echo Отправка файла операторов...
copy DATA\sm%1f68.dat  > NUL
digiwtcp WR 68 %1
del sm%1f68.dat

echo. >> ERR_SEND\clerk
time /t >> ERR_SEND\clerk
copy ERR_SEND\clerk+result ERR_SEND\clerk > NUL