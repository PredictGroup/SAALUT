echo off
ECHO ����祭�� 䠩�� ᮮ�饭�� �ப��⪨...
digiwtcp RD 63 %1
remove_e2 sm%1f63.dat DATA\sm%1f63.dat > NUL
del sm%1f63.dat  
 
echo. >> ERR_GET\scrollmessage
time /t >> ERR_GET\scrollmessage
copy ERR_GET\scrollmessage+result ERR_GET\scrollmessage > NUL


ECHO ����祭�� 䠩�� ��᫥����⥫쭮�� �ப��⪨...
digiwtcp RD 64 %1
remove_e2 sm%1f64.dat DATA\sm%1f64.dat > NUL
del sm%1f64.dat  

echo. >> ERR_GET\scrollseq
time /t >> ERR_GET\scrollseq
copy ERR_GET\scrollseq+result ERR_GET\scrollseq > NUL