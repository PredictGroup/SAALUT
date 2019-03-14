REM	Эта утилита предназначена для копирования файлов данных предназначенных
REM 	для весов SCALE_A в файлы для весов SCALE_B
REM
REM     Синтаксис: 
REM	_копировать_файлы.bat SCALE_A SCALE_B

ECHO OFF
ECHO Копирвание файлов для %1 в файлы для %2

ECHO Копирование файла отделов...
copy DATA\sm%1f32.dat DATA\sm%2f32.dat

ECHO Копирование файла операторов... 
copy DATA\sm%1f68.dat DATA\sm%2f68.dat

ECHO Копирование файла основных групп...
copy DATA\sm%1f35.dat DATA\sm%2f35.dat

ECHO Копирование файла PLU...
copy DATA\sm%1f37.dat DATA\sm%2f37.dat

ECHO Копирование файла форматов этикеток...
copy DATA\sm%1f52.dat DATA\sm%2f52.dat

ECHO Копирование файла данных весов...
copy DATA\sm%1f79.dat DATA\sm%2f79.dat

ECHO Копирование файла текстов...
copy DATA\sm%1f56.dat DATA\sm%2f56.dat

ECHO Копирование файла сообщений прокрутки...
copy DATA\sm%1f63.dat DATA\sm%2f63.dat

ECHO Копирование файла последовательностей прокрутки...
copy DATA\sm%1f64.dat DATA\sm%2f64.dat

ECHO Копирование файла назначенных клавиш...
copy DATA\sm%1f65.dat DATA\sm%2f65.dat

ECHO Копирование файла мета производства...
copy DATA\sm%1f57.dat DATA\sm%2f57.dat

ECHO Копирование файла рекламы...
copy DATA\sm%1f60.dat DATA\sm%2f60.dat

ECHO Копирование файла ингредиентов...
copy DATA\sm%1f58.dat DATA\sm%2f58.dat

ECHO Копирование файла специальных сообщений...
copy DATA\sm%1f59.dat DATA\sm%2f59.dat

ECHO Копирование файла названий магазинов...
copy DATA\sm%1f61.dat DATA\sm%2f61.dat

ECHO Копирование файла логотипов...
copy DATA\sm%1f54.dat DATA\sm%2f54.dat

ECHO Копирование файла названий рисунков...
copy DATA\sm%1f55.dat DATA\sm%2f55.dat
