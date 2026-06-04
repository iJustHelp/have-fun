@echo off
setlocal

@echo Running Have Fun app

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1"
exit /b %ERRORLEVEL%
