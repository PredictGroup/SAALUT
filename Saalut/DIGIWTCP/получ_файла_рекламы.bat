echo off

ECHO Получение файла рекламы...
digiwtcp RD 60 %1 
remove_e2 sm%1f60.dat DATA\sm%1f60.dat > NUL
del sm%1f60.dat  

echo. >> ERR_GET\advert
time /t >> ERR_GET\advert
copy ERR_GET\advert+result ERR_GET\advert > NUL

