@echo off
echo Installing NanoVNA driver...
"%~dp0wdi-simple" --vid 0x0483 --pid 0x5740 --type 3 --name "NanoVNA" --dest "%~dp0NanoVNADriver"
echo.
