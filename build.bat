@echo off
rem Builds CtrlItWorkTimer.exe using the C# compiler that ships with Windows (.NET Framework 4.x).
rem "build.bat ci" only compiles (used by the GitHub release workflow).
cd /d "%~dp0"

set "CI="
if /i "%~1"=="ci" set "CI=1"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo Could not find csc.exe. .NET Framework 4.x is required ^(included with Windows 10/11^).
    if not defined CI pause
    exit /b 1
)

if not defined CI (
    taskkill /im CtrlItWorkTimer.exe /f >nul 2>&1
    taskkill /im WorkTimer.exe /f >nul 2>&1
)

rem The icon is used for the .exe file and also built in for the window, taskbar and About box.
set "ICON="
if exist CtrlItWorkTimer.ico set "ICON=/win32icon:CtrlItWorkTimer.ico /resource:CtrlItWorkTimer.ico,CtrlItWorkTimer.ico"

"%CSC%" /nologo /target:winexe /optimize+ /out:CtrlItWorkTimer.exe %ICON% ^
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
    CtrlItWorkTimer.cs > build.log 2>&1

if errorlevel 1 (
    type build.log
    echo.
    echo Build FAILED - the details are saved in build.log.
    if not defined CI pause
    exit /b 1
)

if defined CI (
    type build.log
    echo Built CtrlItWorkTimer.exe
    exit /b 0
)

echo Built CtrlItWorkTimer.exe - starting it.
start "" "%~dp0CtrlItWorkTimer.exe"
timeout /t 3 >nul
