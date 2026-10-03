@echo off
echo Starting Backend...
cd /d %~dp0backend\src\Api
start "Backend" dotnet run
