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
COPY ["backend/InventoryService/InventoryService.API/InventoryService.API.csproj", "backend/InventoryService/InventoryService.API/"]
COPY ["backend/InventoryService/InventoryService.Application/InventoryService.Application.csproj", "backend/InventoryService/InventoryService.Application/"]
COPY ["backend/InventoryService/InventoryService.Domain/InventoryService.Domain.csproj", "backend/InventoryService/InventoryService.Domain/"]
COPY ["backend/InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj", "backend/InventoryService/InventoryService.Infrastructure/"]

# --- BƯỚC 3: RESTORE KÉO THƯ VIỆN ---
RUN dotnet restore "backend/InventoryService/InventoryService.API/InventoryService.API.csproj"

# --- BƯỚC 4: COPY TOÀN BỘ CODE CÒN LẠI VÀ PUBLISH ---
# Chỉ copy thư mục backend vào, không bê nguyên cả root vào
COPY ["backend/", "backend/"]
WORKDIR "/src/backend/InventoryService/InventoryService.API"

RUN dotnet publish "InventoryService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
COPY --chown=app:app --from=build /app/publish .

USER app

# File chạy cuối cùng
ENTRYPOINT ["dotnet", "InventoryService.API.dll"]