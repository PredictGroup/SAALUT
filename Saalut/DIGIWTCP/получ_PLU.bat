echo off
ECHO Получение файла PLU...
digiwtcp RD 37 %1
remove_e2 sm%1f37.dat DATA\sm%1f37.dat > NUL
del sm%1f37.dat  

echo. >> ERR_GET\plu
time /t >> ERR_GET\plu
copy ERR_GET\PLU+result ERR_GET\PLU > NUL
