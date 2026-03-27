@echo off
setlocal enabledelayedexpansion

:: ======================================================
:: 1. CẤU HÌNH THÔNG TIN
:: ======================================================
:: ======================================================
:: ĐỌC CẤU HÌNH TỪ FILE .ENV
:: ======================================================
if exist .env (
    echo [INFO] Dang nap cau hinh tu file .env...
    for /f "usebackq tokens=*" %%i in (".env") do (
        set "line=%%i"
        :: Bỏ qua dòng trống và dòng comment bắt đầu bằng #
        if not "!line!"=="" (
            set "firstChar=!line:~0,1!"
            if not "!firstChar!"=="#" (
                :: Gán biến trực tiếp và loại bỏ khoảng trắng thừa
                set "%%i"
            )
        )
    )
)
set SONAR_TOKEN=squ_b93c8b22e96b3ec419bc7839c0151668f15c5e6a
set SONAR_URL=http://localhost:9005

:: TỰ ĐỘNG KHÔI PHỤC TOOL
echo --- Dang kiem tra va khoi phuc SonarScanner trong thu muc backend... ---
cd backend
dotnet tool restore >nul 2>&1
cd ..

:MENU
cls
echo ======================================================
echo           CHB FULLSTACK SERVICE SCANNER (v3.1)
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

:: ======================================================
:: 2. LOGIC QUÉT API-GATEWAY (GOLANG)
:: ======================================================
if "%CHOICE%"=="13" (
    set SNAME=api-gateway
    set PROJECT_KEY=chb-api-gateway
    echo.
    echo --- Dang chuan bi quet cho GO: !SNAME! ---
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
:: 3. LOGIC QUÉT BACKEND (.NET) + COVERAGE FIX
:: ======================================================
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

set PROJECT_KEY=chb-%SNAME%
echo.
echo --- [START] Dang phan tich Service: %SNAME% ---

:: Di chuyen vao folder Service (Vidu: backend/EngagementService)
cd backend\%SNAME%

:: DON DEP DU LIEU CU DE TRANH NHAM LAN
if exist ".sonarqube" rd /s /q ".sonarqube"
if exist "TestResults" rd /s /q "TestResults"

:: BƯỚC 3.1: BẮT ĐẦU QUÉT (FIX: Dung wildcard rong nhat de vet file xml)
dotnet sonarscanner begin /k:"%PROJECT_KEY%" ^
  /d:sonar.host.url="%SONAR_URL%" ^
  /d:sonar.token="%SONAR_TOKEN%" ^
  /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" ^
  /d:sonar.exclusions="**/obj/**,**/bin/**,**/Protos/**"

:: BƯỚC 3.2: BUILD
echo --- Dang build cac project trong %SNAME%... ---
for /r %%i in (*.csproj) do (
    dotnet build "%%i" -c Debug -v quiet
)

:: BƯỚC 3.3: CHẠY TEST (FIX: Ep tat ca vao folder TestResults ngay tai goc Service)
echo --- Dang tim va chay Unit Test cho %SNAME%... ---
for /d /r %%d in (*.Tests) do (
    if exist "%%d\*.csproj" (
        echo [TESTING] %%~nxd...
        dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" --results-directory "TestResults" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
    )
)

:: BƯỚC 3.4: KIỂM TRA FILE (Debug log cho )
echo.
echo --- Kiem tra file bao cao tim thay: ---
dir /s /b coverage.opencover.xml
echo.

:: BƯỚC 3.5: KẾT THÚC QUÉT
dotnet sonarscanner end /d:sonar.token="%SONAR_TOKEN%"

cd ..\..

:FINISH
echo.
echo ======================================================
echo HOAN TAT!  mo Dashboard xem %% Coverage nhe.
echo ======================================================
start %SONAR_URL%/dashboard?id=%PROJECT_KEY%
pause
goto MENU