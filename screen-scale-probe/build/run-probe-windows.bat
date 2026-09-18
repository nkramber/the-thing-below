@echo off
rem Runs the screen scale probe on the Windows machine with the 32-inch 1440p screen (D-621).
rem Check the two numbers below. A 32-inch monitor has a diagonal of 31.5 inches in most cases,
rem and the probe draws a ruler on the screen that proves the number.
setlocal
set LABEL=win-32-1440p
set DIAGONAL=31.5
set DISTANCE=70
if not "%~1"=="" set DISTANCE=%~1
echo label %LABEL%, diagonal %DIAGONAL% inches, distance %DISTANCE% cm
"%~dp0ScreenScaleProbe.exe" -- --screen=%LABEL% --diagonal=%DIAGONAL% --distance=%DISTANCE% --report="%~dp0reports"
echo the report is in %~dp0reports
endlocal
