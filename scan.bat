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
echo           CHB FULLSTACK SERVICE SCANNER (v4.0)
echo ======================================================
echo  ID  ^| Service Name (.NET)      ID  ^| Service Name (Go)
echo ------------------------------------------------------
echo  1   ^| AppHost (Toan Bo)         13  ^| API-Gateway
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

:: Đổi working directory vào thư mục service
cd /d "%BASE_DIR%backend\!SNAME!"

:: DỌN DẸP CACHE
if exist ".sonarqube" rd /s /q ".sonarqube"
if exist "TestResults" rd /s /q "TestResults"

:: ======================================================================
:: BƯỚC 3.1: BẮT ĐẦU QUÉT (CHIẾN THUẬT LOẠI TRỪ COVERAGE ĐỘNG)
:: ======================================================================
set "DYNAMIC_EXCLUSIONS=**/*.API/**,**/*.Infrastructure/**,**/*.Domain/**,**/SharedKernel/**,**/*Command.cs,**/*Query.cs,**/*Response.cs,**/*Request.cs,**/Migrations/**"
set "EXCLUDE_BB=true"

:: ĐẶC CÁCH: NẾU LÀ APPHOST HOẶC BUILDING BLOCKS -> CHO PHÉP QUÉT BB
if "!SNAME!"=="AppHost" set "EXCLUDE_BB=false"
if "!SNAME!"=="BuildingBlocks" set "EXCLUDE_BB=false"

if "!EXCLUDE_BB!"=="true" (
    set "DYNAMIC_EXCLUSIONS=!DYNAMIC_EXCLUSIONS!,**/BuildingBlocks/**"
)

:: CHỈ ĐỊNH ĐƯỜNG DẪN COVERAGE
:: Nếu là AppHost, quét sạch file xml trong toàn bộ thư mục backend. Ngược lại chỉ quét trong service hiện tại.
set "COVERAGE_PATH=**/*.xml"
if "!SNAME!"=="AppHost" (
    set "COVERAGE_PATH=%BASE_DIR%backend\**\coverage.opencover.xml"
)

dotnet sonarscanner begin /k:"!PROJECT_KEY!" ^
  /d:sonar.host.url="!SONAR_URL!" ^
  /d:sonar.token="!SONAR_TOKEN!" ^
  /d:sonar.cs.opencover.reportsPaths="!COVERAGE_PATH!" ^
  /d:sonar.coverage.exclusions="!DYNAMIC_EXCLUSIONS!" ^
  /d:sonar.exclusions="**/obj/**,**/bin/**,**/Migrations/**,**/Protos/**"

:: ======================================================================
:: BƯỚC 3.2: BUILD VÀ CHẠY UNIT TEST
:: ======================================================================
if "!SNAME!"=="AppHost" (
    echo --- [AppHost Mode] Dang BUILD toan bo he thong... ---
    :: Build toàn bộ dự án từ thư mục backend
    dotnet build "%BASE_DIR%backend" -c Debug -v quiet

    echo --- [AppHost Mode] Dang chay UNIT TEST toan bo cac Service... ---
    :: Quét và chạy tất cả các project có chữ "Test" trong toàn bộ thư mục backend
    for /d /r "%BASE_DIR%backend" %%d in (*) do (
        set "foldername=%%~nxd"
        echo !foldername! | findstr /i "Test" >nul
        if !errorlevel! == 0 (
            if exist "%%d\*.csproj" (
                echo [RUNNING KÉ TOÀN DỰ ÁN] %%d
                dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
            )
        )
    )
) else (
    echo --- Dang BUILD Service: !SNAME!... ---
    for /r %%i in (*.csproj) do (dotnet build "%%i" -c Debug -v quiet)

    echo --- Dang chay UNIT TEST cho !SNAME!... ---
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
)

:: ======================================================================
:: BƯỚC 3.4: KẾT THÚC VÀ ĐẨY BÁO CÁO
:: ======================================================================
dotnet sonarscanner end /d:sonar.token="!SONAR_TOKEN!"

cd /d "%BASE_DIR%"

:FINISH
echo ======================================================
echo DONE! Check dashboard.
echo ======================================================
start !SONAR_URL!/dashboard?id=!PROJECT_KEY!
pause
goto MENU

@REM ---------------------------------------------------

@REM @echo off
@REM set "BASE_DIR=%~dp0"
@REM cd /d "%BASE_DIR%"
@REM setlocal enabledelayedexpansion

@REM :: ======================================================
@REM :: 1. NẠP CẤU HÌNH TỪ FILE .ENV
@REM :: ======================================================
@REM if exist ".env" (
@REM     echo [INFO] Dang nap cau hinh tu file .env...
@REM     for /f "usebackq tokens=*" %%i in (".env") do (
@REM         set "line=%%i"
@REM         if not "!line!"=="" (
@REM             set "firstChar=!line:~0,1!"
@REM             if not "!firstChar!"=="#" (set "%%i")
@REM         )
@REM     )
@REM )

@REM :: KHÔI PHỤC TOOL
@REM cd /d "%BASE_DIR%backend"
@REM dotnet tool restore >nul 2>&1
@REM cd /d "%BASE_DIR%"

@REM :MENU
@REM cls
@REM echo ======================================================
@REM echo           CHB FULLSTACK SERVICE SCANNER (v3.9)
@REM echo ======================================================
@REM echo  ID  ^| Service Name (.NET)      ID  ^| Service Name (Go)
@REM echo ------------------------------------------------------
@REM echo  1   ^| AppHost                 13  ^| API-Gateway
@REM echo  2   ^| BuildingBlocks
@REM echo  3   ^| EngagementService
@REM echo  4   ^| FranchiseService
@REM echo  5   ^| IdentityService
@REM echo  6   ^| InventoryService
@REM echo  7   ^| NotificationService
@REM echo  8   ^| ProductService
@REM echo  9   ^| SaleService
@REM echo  10  ^| SharedKernel
@REM echo  11  ^| SupplierService
@REM echo  12  ^| WarehouseService         0   ^| Thoat
@REM echo ------------------------------------------------------

@REM set /p CHOICE="Chon ID Service can quet (0-13): "
@REM if "%CHOICE%"=="0" exit

@REM :: LOGIC XỬ LÝ TÊN SERVICE
@REM if "%CHOICE%"=="1" set SNAME=AppHost
@REM if "%CHOICE%"=="2" set SNAME=BuildingBlocks
@REM if "%CHOICE%"=="3" set SNAME=EngagementService
@REM if "%CHOICE%"=="4" set SNAME=FranchiseService
@REM if "%CHOICE%"=="5" set SNAME=IdentityService
@REM if "%CHOICE%"=="6" set SNAME=InventoryService
@REM if "%CHOICE%"=="7" set SNAME=NotificationService
@REM if "%CHOICE%"=="8" set SNAME=ProductService
@REM if "%CHOICE%"=="9" set SNAME=SaleService
@REM if "%CHOICE%"=="10" set SNAME=SharedKernel
@REM if "%CHOICE%"=="11" set SNAME=SupplierService
@REM if "%CHOICE%"=="12" set SNAME=WarehouseService

@REM if not defined SNAME (
@REM     echo Lua chon khong hop le!
@REM     pause
@REM     goto MENU
@REM )

@REM set PROJECT_KEY=chb-!SNAME!
@REM echo.
@REM echo --- [START] Dang phan tich Service: !SNAME! ---

@REM cd /d "%BASE_DIR%backend\!SNAME!"

@REM :: DỌN DẸP CACHE
@REM if exist ".sonarqube" rd /s /q ".sonarqube"
@REM if exist "TestResults" rd /s /q "TestResults"

@REM :: ======================================================================
@REM :: BƯỚC 3.1: BẮT ĐẦU QUÉT (CHIẾN THUẬT LOẠI TRỪ COVERAGE ĐỘNG)
@REM :: ======================================================================
@REM set "DYNAMIC_EXCLUSIONS=**/*.API/**,**/*.Infrastructure/**,**/*.Domain/**,**/SharedKernel/**,**/*Command.cs,**/*Query.cs,**/*Response.cs,**/*Request.cs,**/Migrations/**"

@REM :: --- BỘ LỌC THÔNG MINH CHO BUILDING BLOCKS ---
@REM :: Mặc định: CẤM các service khác quét BuildingBlocks (để giữ coverage độc lập)
@REM set "EXCLUDE_BB=true"

@REM :: ĐẶC CÁCH: NẾU LÀ APPHOST HOẶC BUILDING BLOCKS -> CHO PHÉP QUÉT
@REM if "!SNAME!"=="AppHost" set "EXCLUDE_BB=false"
@REM if "!SNAME!"=="BuildingBlocks" set "EXCLUDE_BB=false"

@REM :: Áp dụng lệnh cấm nếu cờ = true
@REM if "!EXCLUDE_BB!"=="true" (
@REM     set "DYNAMIC_EXCLUSIONS=!DYNAMIC_EXCLUSIONS!,**/BuildingBlocks/**"
@REM )

@REM dotnet sonarscanner begin /k:"!PROJECT_KEY!" ^
@REM   /d:sonar.host.url="!SONAR_URL!" ^
@REM   /d:sonar.token="!SONAR_TOKEN!" ^
@REM   /d:sonar.cs.opencover.reportsPaths="**/*.xml" ^
@REM   /d:sonar.coverage.exclusions="!DYNAMIC_EXCLUSIONS!" ^
@REM   /d:sonar.exclusions="**/obj/**,**/bin/**,**/Migrations/**,**/Protos/**"

@REM :: BƯỚC 3.2: BUILD
@REM for /r %%i in (*.csproj) do (dotnet build "%%i" -c Debug -v quiet)

@REM :: ======================================================================
@REM :: BƯỚC 3.3: CHẠY UNIT TEST
@REM :: ======================================================================
@REM echo --- Running Unit Test for !SNAME!... ---

@REM :: 1. Chạy test của Service hiện tại đang chọn
@REM for /d /r %%d in (*) do (
@REM     set "foldername=%%~nxd"
@REM     echo !foldername! | findstr /i "Test" >nul
@REM     if !errorlevel! == 0 (
@REM         if exist "%%d\*.csproj" (
@REM             echo [RUNNING] %%d
@REM             dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
@REM         )
@REM     )
@REM )

@REM :: 2. NẾU LÀ APPHOST -> Chạy thêm Unit Test của BuildingBlocks
@REM if "!SNAME!"=="AppHost" (
@REM     echo --- [AppHost Mode] Dang chay test cua BuildingBlocks... ---
@REM     :: Build BB trước cho chắc ăn
@REM     dotnet build "%BASE_DIR%backend\BuildingBlocks" -c Debug -v quiet
    
@REM     for /d /r "%BASE_DIR%backend\BuildingBlocks" %%d in (*) do (
@REM         set "foldername=%%~nxd"
@REM         echo !foldername! | findstr /i "Test" >nul
@REM         if !errorlevel! == 0 (
@REM             if exist "%%d\*.csproj" (
@REM                 echo [RUNNING KÉ] %%d
@REM                 dotnet test "%%d" --no-build --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
@REM             )
@REM         )
@REM     )
@REM )

@REM :: BƯỚC 3.4: KẾT THÚC
@REM dotnet sonarscanner end /d:sonar.token="!SONAR_TOKEN!"

@REM cd /d "%BASE_DIR%"

@REM :FINISH
@REM echo ======================================================
@REM echo DONE! Check dashboard.
@REM echo ======================================================
@REM start !SONAR_URL!/dashboard?id=!PROJECT_KEY!
@REM pause
@REM goto MENU