echo off

ECHO ����祭�� 䠩�� �⤥���...
digiwtcp RD 32 %1 
remove_e2 sm%1f32.dat DATA\sm%1f32.dat > NUL
del sm%1f32.dat  

echo. >> ERR_GET\department
time /t >> ERR_GET\department
copy ERR_GET\department+result ERR_GET\department > NUL

