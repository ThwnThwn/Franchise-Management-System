@echo off
set "BASE_DIR=%~dp0"
cd /d "%BASE_DIR%"
setlocal enabledelayedexpansion

:: ======================================================
:: 1. NẠP CẤU HÌNH TỪ FILE .ENV
:: ======================================================
if exist ".env" (
    echo [INFO] Dang nap cau hinh tu file .env...
    for /f "usebackq tokens=*" %%i in (".env") do (
        set "line=%%i"
        if not "!line!"=="" (
            set "firstChar=!line:~0,1!"
            if not "!firstChar!"=="#" (set "%%i")
        )
    )
)

:: KHÔI PHỤC TOOL
cd /d "%BASE_DIR%backend"
dotnet tool restore >nul 2>&1
cd /d "%BASE_DIR%"

:MENU
cls
echo ======================================================
echo           CHB FULLSTACK SERVICE SCANNER (v3.9)
echo ======================================================
echo  ID  ^| Service Name (.NET)      ID  ^| Service Name (Go)
echo ------------------------------------------------------
echo  1   ^| AppHost                 13  ^| API-Gateway
echo  2   ^| BuildingBlocks
echo  3   ^| EngagementService
echo  4   ^| FranchiseService
echo  5   ^| IdentityService
echo  6   ^| InventoryService
echo  7   ^| NotificationService
echo  8   ^| ProductService
echo  9   ^| SaleService
echo  10  ^| SharedKernel
echo  11  ^| SupplierService
echo  12  ^| WarehouseService         0   ^| Thoat
echo ------------------------------------------------------

set /p CHOICE="Chon ID Service can quet (0-13): "
if "%CHOICE%"=="0" exit

:: LOGIC XỬ LÝ TÊN SERVICE
if "%CHOICE%"=="1" set SNAME=AppHost
if "%CHOICE%"=="2" set SNAME=BuildingBlocks
if "%CHOICE%"=="3" set SNAME=EngagementService
if "%CHOICE%"=="4" set SNAME=FranchiseService
if "%CHOICE%"=="5" set SNAME=IdentityService
if "%CHOICE%"=="6" set SNAME=InventoryService
if "%CHOICE%"=="7" set SNAME=NotificationService
if "%CHOICE%"=="8" set SNAME=ProductService
if "%CHOICE%"=="9" set SNAME=SaleService
if "%CHOICE%"=="10" set SNAME=SharedKernel
if "%CHOICE%"=="11" set SNAME=SupplierService
if "%CHOICE%"=="12" set SNAME=WarehouseService

if not defined SNAME (
    echo Lua chon khong hop le!
    pause
    goto MENU
)

set PROJECT_KEY=chb-!SNAME!
echo.
echo --- [START] Dang phan tich Service: !SNAME! ---

cd /d "%BASE_DIR%backend\!SNAME!"

:: DỌN DẸP CACHE
if exist ".sonarqube" rd /s /q ".sonarqube"
if exist "TestResults" rd /s /q "TestResults"

:: BƯỚC 3.1: BẮT ĐẦU QUÉT (CHIẾN THUẬT LOẠI TRỪ COVERAGE)
:: - sonar.sources=".": Quét TOÀN BỘ để tìm Code Smell (Bao gồm cả SharedKernel nếu có reference)
:: - sonar.coverage.exclusions: LOẠI BỎ các tầng không phải logic khỏi bảng tính % Coverage
dotnet sonarscanner begin /k:"!PROJECT_KEY!" ^
  /d:sonar.host.url="!SONAR_URL!" ^
  /d:sonar.token="!SONAR_TOKEN!" ^
  /d:sonar.cs.opencover.reportsPaths="**/*.xml" ^
  /d:sonar.coverage.exclusions="**/*.API/**,**/*.Infrastructure/**,**/*.Domain/**,**/SharedKernel/**,**/*Command.cs,**/*Query.cs,**/*Response.cs,**/*Request.cs,**/Migrations/**" ^
  /d:sonar.exclusions="**/obj/**,**/bin/**,**/Migrations/**,**/Protos/**"

:: BƯỚC 3.2: BUILD
for /r %%i in (*.csproj) do (dotnet build "%%i" -c Debug -v quiet)

:: BƯỚC 3.3: CHẠY UNIT TEST
echo --- Dang chay Unit Test cho !SNAME!... ---
for /d /r %%d in (*) do (
    set "foldername=%%~nxd"
    echo !foldername! | findstr /i "Test" >nul
    if !errorlevel! == 0 (
        if exist "%%d\*.csproj" (
            dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
        )
    )
)

:: BƯỚC 3.4: KẾT THÚC
dotnet sonarscanner end /d:sonar.token="!SONAR_TOKEN!"

cd /d "%BASE_DIR%"

:FINISH
echo ======================================================
echo HOAN TAT! Duy kiem tra Dashboard nhe.
echo ======================================================
start !SONAR_URL!/dashboard?id=!PROJECT_KEY!
pause
goto MENU