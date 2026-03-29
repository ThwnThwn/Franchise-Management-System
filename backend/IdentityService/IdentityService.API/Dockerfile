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
COPY ["backend/IdentityService/IdentityService.API/IdentityService.API.csproj", "backend/IdentityService/IdentityService.API/"]
COPY ["backend/IdentityService/IdentityService.Application/IdentityService.Application.csproj", "backend/IdentityService/IdentityService.Application/"]
COPY ["backend/IdentityService/IdentityService.Domain/IdentityService.Domain.csproj", "backend/IdentityService/IdentityService.Domain/"]
COPY ["backend/IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "backend/IdentityService/IdentityService.Infrastructure/"]

# --- BƯỚC 3: RESTORE KÉO THƯ VIỆN ---
RUN dotnet restore "backend/IdentityService/IdentityService.API/IdentityService.API.csproj"

# --- BƯỚC 4: COPY TOÀN BỘ CODE CÒN LẠI VÀ PUBLISH ---
# Chỉ copy thư mục backend vào, không bê nguyên cả root vào
COPY ["backend/", "backend/"]
WORKDIR "/src/backend/IdentityService/IdentityService.API"

RUN dotnet publish "IdentityService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "IdentityService.API.dll"]