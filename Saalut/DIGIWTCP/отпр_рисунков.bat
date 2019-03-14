echo off
echo Отправка рисунков...
copy DATA\sm%1f55.dat  > NUL
digiwtcp WR 55 %1
del sm%1f55.dat

echo. >> ERR_SEND\image
time /t >> ERR_SEND\image
copy ERR_SEND\image+result ERR_SEND\image > NUL