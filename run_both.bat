@echo off
echo Starting Backend and Frontend...

start "Backend" cmd /c "cd /d %~dp0backend\src\Api && dotnet run"
start "Frontend" cmd /c "cd /d %~dp0frontend && npm start"
