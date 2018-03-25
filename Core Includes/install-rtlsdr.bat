@echo off

mkdir tmp

echo Downloading RTLSDR Driver
httpget http://osmocom.org/attachments/download/2242/RelWithDebInfo.zip tmp\RelWithDebInfo.zip

echo Downloading Zadig
set zadig_exe=zadig.exe
ver | findstr /l "5.1." > NUL
if %errorlevel% equ 0 set zadig_exe=zadig_xp.exe
httpget http://zadig.akeo.ie/downloads/%zadig_exe% %zadig_exe%

unzip -o tmp\RelWithDebInfo.zip -d tmp
move tmp\rtl-sdr-release\x32\rtlsdr.dll .

rmdir tmp /S /Q