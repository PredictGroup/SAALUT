echo off
ECHO Получение рисунков...
digiwtcp RD 55 %1
remove_e2 sm%1f55.dat DATA\sm%1f55.dat > NUL
del sm%1f55.dat  


echo. >> ERR_GET\image
time /t >> ERR_GET\image
copy ERR_GET\image+result ERR_GET\image > NUL