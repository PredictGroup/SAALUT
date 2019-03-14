echo off

ECHO Получение файла весов...
digiwtcp RD 79 %1 
remove_e2 sm%1f79.dat DATA\sm%1f79.dat > NUL
del sm%1f79.dat  

echo. >> ERR_GET\scalefile
time /t >> ERR_GET\scalefile
copy ERR_GET\scalefile+result ERR_GET\scalefile > NUL
