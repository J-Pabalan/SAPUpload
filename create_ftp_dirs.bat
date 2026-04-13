@echo off
echo Creating FTP directory structure...
echo.
echo Connecting to FTP server to create directories...
echo.

REM Create FTP script file
echo open 10.87.36.63 > ftp_create.txt
echo admin >> ftp_create.txt
echo @2021PmPcN@$Adm!n*.* >> ftp_create.txt
echo mkdir SAP-FTP >> ftp_create.txt
echo cd SAP-FTP >> ftp_create.txt
echo mkdir OUTBOUND >> ftp_create.txt
echo cd OUTBOUND >> ftp_create.txt
echo mkdir UPLOADED >> ftp_create.txt
echo pwd >> ftp_create.txt
echo ls >> ftp_create.txt
echo bye >> ftp_create.txt

REM Execute FTP commands
ftp -s:ftp_create.txt

REM Clean up
del ftp_create.txt

echo.
echo Directory creation attempt complete!
echo If you see "550" or "failed" messages above, you need to create directories via SMB or contact your FTP admin.
pause
