@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\dev.ps1" -Configuration Release
set "AFM_EXIT=%ERRORLEVEL%"
if not "%AFM_EXIT%"=="0" (
    echo.
    echo No se pudo iniciar AdbFileManager. Revisa el error mostrado arriba.
    pause
)
exit /b %AFM_EXIT%
