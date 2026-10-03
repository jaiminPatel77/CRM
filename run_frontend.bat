@echo off
echo Starting Frontend...
cd /d %~dp0frontend
start "Frontend" npm start
