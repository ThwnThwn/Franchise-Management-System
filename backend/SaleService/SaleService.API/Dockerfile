# ==========================================
# STAGE 1: BUILD (Dùng SDK để compile)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- BƯỚC 1: COPY PROTOS & CLASS LIBRARY DÙNG CHUNG ---
COPY ["protos/", "protos/"]
COPY ["backend/BuildingBlocks/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "backend/BuildingBlocks/BuildingBlocks.Core/"]
COPY ["backend/SharedKernel/SharedKernel.csproj", "backend/SharedKernel/"]

# --- BƯỚC 2: COPY PROJECT CỦA SERVICE HIỆN TẠI ---
COPY ["backend/SaleService/SaleService.API/SalesService.API.csproj", "backend/SaleService/SaleService.API/"]
COPY ["backend/SaleService/SaleService.Application/SalesService.Application.csproj", "backend/SaleService/SaleService.Application/"]
COPY ["backend/SaleService/SaleService.Domain/SalesService.Domain.csproj", "backend/SaleService/SaleService.Domain/"]
COPY ["backend/SaleService/SaleService.Infrastructure/SalesService.Infrastructure.csproj", "backend/SaleService/SaleService.Infrastructure/"]

# --- BƯỚC 3: RESTORE KÉO THƯ VIỆN ---
RUN dotnet restore "backend/SaleService/SaleService.API/SalesService.API.csproj"

# --- BƯỚC 4: COPY TOÀN BỘ CODE CÒN LẠI VÀ PUBLISH ---
# Chỉ copy thư mục backend vào, không bê nguyên cả root vào
COPY ["backend/", "backend/"]
WORKDIR "/src/backend/SaleService/SaleService.API"

RUN dotnet publish "SalesService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# STAGE 2: RUNTIME (Dùng Alpine siêu nhẹ)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache tzdata

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy kết quả compile từ Stage 1 sang
COPY --from=build /app/publish .

# File chạy cuối cùng
ENTRYPOINT ["dotnet", "SalesService.API.dll"]