echo off
ECHO Получение логотипов...
digiwtcp RD 54 %1
remove_e2 sm%1f54.dat DATA\sm%1f54.dat > NUL
del sm%1f54.dat  
  

echo. >> ERR_GET\logo
time /t >> ERR_GET\logo
copy ERR_GET\logo+result ERR_GET\logo > NUL