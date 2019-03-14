echo off
ECHO Отправка файла отдела....
copy DATA\sm%1f32.dat  > NUL
digiwtcp WR 32 %1
del sm%1f32.dat

echo. >> ERR_SEND\department
time /t >> ERR_SEND\department
copy ERR_SEND\department+result ERR_SEND\department > NUL