echo off
ECHO Отправка сообщений прокрутки...
copy DATA\sm%1f63.dat  > NUL
digiwtcp WR 63 %1
del sm%1f63.dat

echo. >> ERR_SEND\scrollmessage
time /t >> ERR_SEND\scrollmessage
copy ERR_SEND\scrollmessage+result ERR_SEND\scrollmessage > NUL

ECHO Отправка последовательностей прокрутки...
copy DATA\sm%1f64.dat  > NUL
digiwtcp WR 64 %1
del sm%1f64.dat

echo. >> ERR_SEND\scrollseq
time /t >> ERR_SEND\scrollseq
copy ERR_SEND\scrollseq+result ERR_SEND\scrollseq > NUL