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
COPY ["backend/EngagementService/EngagementService.API/EngagementService.API.csproj", "backend/EngagementService/EngagementService.API/"]
COPY ["backend/EngagementService/EngagementService.Application/EngagementService.Application.csproj", "backend/EngagementService/EngagementService.Application/"]
COPY ["backend/EngagementService/EngagementService.Domain/EngagementService.Domain.csproj", "backend/EngagementService/EngagementService.Domain/"]
COPY ["backend/EngagementService/EngagementService.Infrastucture/EngagementService.Infrastucture.csproj", "backend/EngagementService/EngagementService.Infrastucture/"]

# --- BƯỚC 3: RESTORE KÉO THƯ VIỆN ---
RUN dotnet restore "backend/EngagementService/EngagementService.API/EngagementService.API.csproj"

# --- BƯỚC 4: COPY TOÀN BỘ CODE CÒN LẠI VÀ PUBLISH ---
# Chỉ copy thư mục backend vào, không bê nguyên cả root vào
COPY ["backend/", "backend/"]
WORKDIR "/src/backend/EngagementService/EngagementService.API"

RUN dotnet publish "EngagementService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# STAGE 2: RUNTIME (Dùng Alpine siêu nhẹ)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy kết quả compile từ Stage 1 sang
COPY --from=build /app/publish .

# File chạy cuối cùng
ENTRYPOINT ["dotnet", "EngagementService.API.dll"]