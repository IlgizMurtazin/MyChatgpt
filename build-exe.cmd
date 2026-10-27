@echo off
setlocal
pushd "%~dp0"

dotnet publish "src\BlackCalculator\BlackCalculator.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:NuGetLockFilePath=obj/publish.packages.lock.json -o "artifacts\win-x64"
if errorlevel 1 (
    echo.
    echo Build failed. See the error above. This project requires the .NET 10 SDK.
    popd
    pause
    exit /b 1
)

echo.
echo Ready: %CD%\artifacts\win-x64\BlackCalculator.exe
popd
pause
exit /b 0
