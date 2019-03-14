echo off

:START

call _получ_все.bat %1
ECHO.
ECHO.
call _отпр_все.bat %1

ECHO.
ECHO.

goto START