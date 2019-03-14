echo off

ECHO Отправка основной группы...
copy DATA\sm%1f35.dat  > NUL
digiwtcp WR 35 %1
del sm%1f35.dat

echo. >> ERR_SEND\maingroup
time /t >> ERR_SEND\maingroup
copy ERR_SEND\maingroup+result ERR_SEND\maingroup > NUL