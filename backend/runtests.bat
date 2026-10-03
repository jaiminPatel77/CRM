@echo off
SETLOCAL EnableDelayedExpansion

:: --- Configuration ---
SET SOLUTION_FILE=Crm.sln
SET RESULTS_DIR=TestResults
SET COVERAGE_REPORT_DIR=%RESULTS_DIR%\CoverageReport

echo ============================================================
echo   Crm - Backend Test Runner
echo ============================================================
echo.

:: Check if Solution file exists
if not exist "%SOLUTION_FILE%" (
    echo [ERROR] Solution file "%SOLUTION_FILE%" not found!
    echo Please run this script from the project root.
    pause
    exit /b 1
)

:: Clean old results
if exist "%RESULTS_DIR%" (
    echo [INFO] Cleaning old test results...
    rmdir /s /q "%RESULTS_DIR%"
)

echo [INFO] Running all tests in solution with Code Coverage...
echo.

:: Run dotnet test
:: --collect "XPlat Code Coverage" requires the 'coverlet.collector' NuGet package in test projects.
:: --logger "trx" generates a standard Visual Studio results file.
dotnet test "%SOLUTION_FILE%" ^
    --configuration Debug ^
    --logger "trx;LogFileName=TestResults.trx" ^
    --results-directory "%RESULTS_DIR%" ^
    --collect "XPlat Code Coverage" ^
    -nologo

set EXIT_CODE=%errorlevel%

echo.
if %EXIT_CODE% neq 0 (
    echo ============================================================
    echo   [FAILURE] Some tests failed ^(Exit Code: %EXIT_CODE%^)
    echo ============================================================
) else (
    echo ============================================================
    echo   [SUCCESS] All tests passed!
    echo ============================================================
)

echo.
echo Test results are available in: %RESULTS_DIR%
echo To view coverage, look for 'coverage.cobertura.xml' in the subdirectories of %RESULTS_DIR%.
echo.

pause
ENDLOCAL
exit /b %EXIT_CODE%
