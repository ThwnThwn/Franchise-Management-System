
@echo off
setlocal enabledelayedexpansion

:: ======================================================
:: CẤU HÌNH THÔNG TIN (thay Token của Duy vào đây)
:: ======================================================
set SONAR_TOKEN=squ_ff1fc2526bf6b75680247b1477c91f6c6c426e01
set SONAR_URL=http://localhost:9005

:: TỰ ĐỘNG RESTORE LOCAL TOOL
echo --- Dang kiem tra va khoi phuc SonarScanner trong thu muc backend... ---
cd backend
dotnet tool restore
cd ..

:MENU
cls
echo ======================================================
echo           CHB BACKEND SERVICE SCANNER (LOCAL TOOL)
echo ======================================================
echo  ID  ^| Service Name
echo ------------------------------------------------------
echo  1   ^| AppHost             7   ^| NotificationService
echo  2   ^| BuildingBlocks      8   ^| ProductService
echo  3   ^| EngagementService   9   ^| SaleService
echo  4   ^| FranchiseService    10  ^| SharedKernel
echo  5   ^| IdentityService     11  ^| SupplierService
echo  6   ^| InventoryService    12  ^| WarehouseService
echo  0   ^| Thoat               13  ^| API-Gateway
echo ------------------------------------------------------

set /p CHOICE="Chon ID Service can quet (0-13): "

if "%CHOICE%"=="0" exit

:: ======================================================
:: LOGIC QUÉT API-GATEWAY (GOLANG)
:: ======================================================
if "%CHOICE%"=="13" (
    set SNAME=api-gateway
    set PROJECT_KEY=chb-api-gateway
    echo.
    echo --- Dang chuan bi quet cho GO: !SNAME! ---
    
    :: Chạy Scanner CLI qua Docker (Sạch sẽ, không cần cài Java/Go Scanner vào máy)
    :: Lưu ý: Dùng host.docker.internal để Container kết nối được SonarQube trên Host
    docker run --rm ^
      -v "%CD%\api-gateway:/usr/src" ^
      sonarsource/sonar-scanner-cli ^
      -Dsonar.projectKey=!PROJECT_KEY! ^
      -Dsonar.sources=. ^
      -Dsonar.host.url=http://host.docker.internal:9005 ^
      -Dsonar.login=%SONAR_TOKEN%
    
    goto FINISH
)

:: ======================================================
:: LOGIC QUÉT BACKEND (.NET)
:: ======================================================
if "%CHOICE%"=="1" set SNAME=AppHost
if "%CHOICE%"=="2" set SNAME=BuildingBlocks
if "%CHOICE%"=="3" set SNAME=EngagementService
if "%CHOICE%"=="4" set SNAME=FranchiseService
if "%CHOICE%"=="5" set SNAME=IdentityService
if "%CHOICE%"=="6" set SNAME=InventoryService
if "%CHOICE%"=="7" set SNAME=NotificationService
if "%CHOICE%"=="8" set SNAME=ProductService
if "%CHOICE%"=="9" set SNAME=SalesService
if "%CHOICE%"=="10" set SNAME=SharedKernel
if "%CHOICE%"=="11" set SNAME=SupplierService
if "%CHOICE%"=="12" set SNAME=WarehouseService

if not defined SNAME (
    echo Lua chon khong hop le! Nhan phim bat ky de thu lai.
    pause >nul
    goto MENU
)

set PROJECT_KEY=chb-%SNAME%

echo.
echo --- Dang chuan bi quet cho .NET: %SNAME% ---

cd backend\%SNAME%
if exist ".sonarqube" rd /s /q ".sonarqube"

:: ======================================================================
:: BƯỚC 3.1: BẮT ĐẦU QUÉT (CHIẾN THUẬT LOẠI TRỪ COVERAGE ĐỘNG)
:: ======================================================================
set "DYNAMIC_EXCLUSIONS=**/*.API/**,**/*.Infrastructure/**,**/*.Domain/**,**/SharedKernel/**,**/*Command.cs,**/*Query.cs,**/*Response.cs,**/*Request.cs,**/Migrations/**"

:: --- BỘ LỌC THÔNG MINH CHO BUILDING BLOCKS ---
:: Mặc định: CẤM các service khác quét BuildingBlocks (để giữ coverage độc lập)
set "EXCLUDE_BB=true"

:: ĐẶC CÁCH: NẾU LÀ APPHOST HOẶC BUILDING BLOCKS -> CHO PHÉP QUÉT
if "!SNAME!"=="AppHost" set "EXCLUDE_BB=false"
if "!SNAME!"=="BuildingBlocks" set "EXCLUDE_BB=false"

:: Áp dụng lệnh cấm nếu cờ = true
if "!EXCLUDE_BB!"=="true" (
    set "DYNAMIC_EXCLUSIONS=!DYNAMIC_EXCLUSIONS!,**/BuildingBlocks/**"
)

dotnet sonarscanner begin /k:"!PROJECT_KEY!" ^
  /d:sonar.host.url="!SONAR_URL!" ^
  /d:sonar.token="!SONAR_TOKEN!" ^
  /d:sonar.cs.opencover.reportsPaths="**/*.xml" ^
  /d:sonar.coverage.exclusions="!DYNAMIC_EXCLUSIONS!" ^
  /d:sonar.exclusions="**/obj/**,**/bin/**,**/Migrations/**,**/Protos/**"

:: BƯỚC 3.2: BUILD
for /r %%i in (*.csproj) do (dotnet build "%%i" -c Debug -v quiet)

:: ======================================================================
:: BƯỚC 3.3: CHẠY UNIT TEST
:: ======================================================================
echo --- Running Unit Test for !SNAME!... ---

:: 1. Chạy test của Service hiện tại đang chọn
for /d /r %%d in (*) do (
    set "foldername=%%~nxd"
    echo !foldername! | findstr /i "Test" >nul
    if !errorlevel! == 0 (
        if exist "%%d\*.csproj" (
            echo [RUNNING] %%d
            dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
        )
    )
)

:: 2. NẾU LÀ APPHOST -> Chạy thêm Unit Test của BuildingBlocks
if "!SNAME!"=="AppHost" (
    echo --- [AppHost Mode] Dang chay test cua BuildingBlocks... ---
    :: Build BB trước cho chắc ăn
    dotnet build "%BASE_DIR%backend\BuildingBlocks" -c Debug -v quiet
    
    for /d /r "%BASE_DIR%backend\BuildingBlocks" %%d in (*) do (
        set "foldername=%%~nxd"
        echo !foldername! | findstr /i "Test" >nul
        if !errorlevel! == 0 (
            if exist "%%d\*.csproj" (
                echo [RUNNING KÉ] %%d
                dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
            )
        )
    )
)

:: BƯỚC 3.4: KẾT THÚC
dotnet sonarscanner end /d:sonar.token="!SONAR_TOKEN!"

cd /d "%BASE_DIR%"

:FINISH
echo.
echo ======================================================
echo HOAN TAT QUET %SNAME%!
echo ======================================================
echo.

:: Mo trinh duyet xem dashboard
start %SONAR_URL%/dashboard?id=%PROJECT_KEY%

echo Nhan phim bat ky de quay lai Menu...
pause >nul
goto MENU