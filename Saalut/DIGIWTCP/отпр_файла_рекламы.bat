echo off
ECHO Отправка файла рекламы...
copy DATA\sm%1f60.dat  > NUL
digiwtcp WR 60 %1
del sm%1f60.dat

echo. >> ERR_SEND\advert
time /t >> ERR_SEND\advert
copy ERR_SEND\advert+result ERR_SEND\advert > NUL