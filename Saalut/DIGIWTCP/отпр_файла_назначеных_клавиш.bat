echo off
ECHO Отправка файла назначеных клавиш...
copy DATA\sm%1f65.dat  > NUL
digiwtcp WR 65 %1
del sm%1f65.dat

echo. >> ERR_SEND\preset
time /t >> ERR_SEND\preset
copy ERR_SEND\preset+result ERR_SEND\preset > NUL
