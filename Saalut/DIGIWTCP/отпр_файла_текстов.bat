echo off
ECHO ��ࠢ�� 䠩�� ⥪�⮢....
copy DATA\sm%1f56.dat  > NUL
digiwtcp WR 56 %1
del sm%1f56.dat

echo. >> ERR_SEND\text
time /t >> ERR_SEND\text
copy ERR_SEND\text+result ERR_SEND\text > NUL