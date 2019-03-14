echo off
ECHO Получение файла места производства...
digiwtcp RD 57 %1
remove_e2 sm%1f57.dat DATA\sm%1f57.dat > NUL
del sm%1f57.dat  

echo. >> ERR_GET\place
time /t >> ERR_GET\place
copy ERR_GET\place+result ERR_GET\place > NUL


