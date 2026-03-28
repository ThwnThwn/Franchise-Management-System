
@echo off
setlocal enabledelayedexpansion

:: ======================================================
:: CẤU HÌNH THÔNG TIN (thay Token của Duy vào đây)
:: ======================================================
set SONAR_TOKEN=squ_e606a79c62a30bdcf3c46d93f22057bd6e3315cb
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

:: BAT DAU QUET .NET
dotnet sonarscanner begin /k:"%PROJECT_KEY%" /d:sonar.host.url="%SONAR_URL%" /d:sonar.login="%SONAR_TOKEN%" /d:sonar.exclusions="**/obj/**,**/bin/**,**/Protos/**"

:: BUILD
echo --- Dang build cac project trong %SNAME%... ---
for /r %%i in (*.csproj) do (
    dotnet build "%%i" -c Debug
)

:: KET THUC QUET .NET
dotnet sonarscanner end /d:sonar.login="%SONAR_TOKEN%"
cd ..\..

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