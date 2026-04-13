@echo off
echo ========================================
echo Enable SMB Guest Access (For Testing)
echo ========================================
echo.
echo This allows unauthenticated SMB guest access.
echo WARNING: Only use this for testing on trusted networks!
echo.
pause

echo Adding registry key to allow guest access...
reg add "HKLM\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters" /v AllowInsecureGuestAuth /t REG_DWORD /d 1 /f

echo.
echo ========================================
echo DONE! Registry key added.
echo.
echo IMPORTANT: You need to RESTART or restart the Workstation service.
echo To restart service: net stop workstation /y && net start workstation
echo ========================================
pause
