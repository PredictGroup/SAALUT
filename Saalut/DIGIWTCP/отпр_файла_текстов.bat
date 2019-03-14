echo off
ECHO β―ΰ Άª  δ ©«  β¥ªαβ®Ά....
copy DATA\sm%1f56.dat  > NUL
digiwtcp WR 56 %1
del sm%1f56.dat

echo. >> ERR_SEND\text
time /t >> ERR_SEND\text
copy ERR_SEND\text+result ERR_SEND\text > NUL