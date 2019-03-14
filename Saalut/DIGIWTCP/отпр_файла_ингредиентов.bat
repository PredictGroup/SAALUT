echo off
ECHO Отправка файла ингредентов...
copy DATA\sm%1f58.dat  > NUL
digiwtcp WR 58 %1
del sm%1f58.dat

echo. >> ERR_SEND\ingred
time /t >> ERR_SEND\ingred
copy ERR_SEND\ingred+result ERR_SEND\ingred > NUL