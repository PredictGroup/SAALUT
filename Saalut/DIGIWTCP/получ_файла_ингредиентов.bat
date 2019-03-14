echo off

ECHO Получение файла ингредиентов...
digiwtcp RD 58 %1
remove_e2 sm%1f58.dat DATA\sm%1f58.dat > NUL
del sm%1f58.dat  

echo. >> ERR_GET\ingred
time /t >> ERR_GET\ingred
copy ERR_GET\ingred+result ERR_GET\ingred > NUL


