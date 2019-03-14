echo off

ECHO Получение файла назначенных клавиш...
digiwtcp RD 65 %1 
remove_e2 sm%1f65.dat DATA\sm%1f65.dat > NUL
del sm%1f65.dat  

echo. >> ERR_GET\preset
time /t >> ERR_GET\preset
copy ERR_GET\preset+result ERR_GET\preset > NUL
