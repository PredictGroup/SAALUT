echo off
ECHO Получение файла групп...
digiwtcp RD 35 %1
remove_e2 sm%1f35.dat DATA\sm%1f35.dat > NUL
del sm%1f35.dat  
 

echo. >> ERR_GET\maingroup
time /t >> ERR_GET\maingroup
copy ERR_GET\maingroup+result ERR_GET\maingroup > NUL