@echo off
rem Runs the screen scale probe on a 1920 by 1080 screen (D-621, OQ-183).
rem Correct the diagonal below for the screen. A 24-inch monitor measures 23.8 inches in
rem most cases, and a 27-inch monitor measures 27.0. The ruler bar on the frame proves it.
rem The first argument sets the distance in centimeters, and the second sets the diagonal.
rem The probe starts in fit mode fill, which is the fit of D-573. Press F or the right
rem shoulder button of a pad to see fit mode whole, which holds the frame at 1x with bars.
setlocal
set LABEL=win-1080p
set DIAGONAL=23.8
set DISTANCE=70
if not "%~1"=="" set DISTANCE=%~1
if not "%~2"=="" set DIAGONAL=%~2
echo label %LABEL%, diagonal %DIAGONAL% inches, distance %DISTANCE% cm
"%~dp0ScreenScaleProbe.exe" -- --screen=%LABEL% --diagonal=%DIAGONAL% --distance=%DISTANCE% --report="%~dp0reports"
echo the report is in %~dp0reports
endlocal
