echo off

ECHO Получение файла форматов печати...
digiwtcp RD 52 %1 
remove_e2 sm%1f52.dat DATA\sm%1f52.dat > NUL
del sm%1f52.dat  

echo. >> ERR_GET\freeformat
time /t >> ERR_GET\freeformat
copy ERR_GET\freeformat+result ERR_GET\freeformat > NUL

