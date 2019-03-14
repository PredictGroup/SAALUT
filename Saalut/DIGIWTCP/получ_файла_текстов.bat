echo off

ECHO Получение файла текстов...
digiwtcp RD 56 %1 
remove_e2 sm%1f56.dat DATA\sm%1f56.dat > NUL
del sm%1f56.dat  


echo. >> ERR_GET\text
time /t >> ERR_GET\text
copy ERR_GET\text+result ERR_GET\text > NUL