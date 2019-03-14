echo off
echo Отправка файла весов...
copy DATA\sm%1f79.dat  > NUL
digiwtcp WR 79 %1
del sm%1f79.dat

echo. >> ERR_SEND\scalefile
time /t >> ERR_SEND\scalefile
copy ERR_SEND\scalefile+result ERR_SEND\scalefile > NUL
