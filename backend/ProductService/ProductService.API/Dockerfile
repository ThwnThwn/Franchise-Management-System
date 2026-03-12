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
COPY ["backend/ProductService/ProductService.API/ProductService.API.csproj", "backend/ProductService/ProductService.API/"]
COPY ["backend/ProductService/ProductService.Application/ProductService.Application.csproj", "backend/ProductService/ProductService.Application/"]
COPY ["backend/ProductService/ProductService.Domain/ProductService.Domain.csproj", "backend/ProductService/ProductService.Domain/"]
COPY ["backend/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "backend/ProductService/ProductService.Infrastructure/"]

# --- BƯỚC 3: RESTORE KÉO THƯ VIỆN ---
RUN dotnet restore "backend/ProductService/ProductService.API/ProductService.API.csproj"

# --- BƯỚC 4: COPY TOÀN BỘ CODE CÒN LẠI VÀ PUBLISH ---
# Chỉ copy thư mục backend vào, không bê nguyên cả root vào
COPY ["backend/", "backend/"]
WORKDIR "/src/backend/ProductService/ProductService.API"

RUN dotnet publish "ProductService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "ProductService.API.dll"]