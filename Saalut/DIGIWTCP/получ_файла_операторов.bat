echo off

ECHO Получение файла операторов...
digiwtcp RD 68 %1 
remove_e2 sm%1f68.dat DATA\sm%1f68.dat > NUL
del sm%1f68.dat  


echo. >> ERR_GET\clerk
time /t >> ERR_GET\clerk
copy ERR_GET\clerk+result ERR_GET\clerk > NUL