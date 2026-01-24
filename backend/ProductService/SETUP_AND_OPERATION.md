# Product Service - Hướng Dẫn Setup và Quy Trình Hoạt Động

## 📋 Mục Lục
1. [Tổng Quan](#tổng-quan)
2. [Kiến Trúc Clean Architecture](#kiến-trúc-clean-architecture)
3. [Cấu Hình Cần Thiết trong Program.cs](#cấu-hình-cần-thiết-trong-programcs)
4. [Cấu Hình Port](#cấu-hình-port)
5. [Quy Trình Hoạt Động](#quy-trình-hoạt-động)
6. [Dependencies và Packages](#dependencies-và-packages)
7. [API Endpoints](#api-endpoints)

---

## 🎯 Tổng Quan

**Product Service** là một microservice được xây dựng theo **Clean Architecture**, hỗ trợ cả **REST API** và **gRPC** để:
- Quản lý sản phẩm (Products)
- Quản lý biến thể sản phẩm (Product Variants - Size, Giá)
- Lọc sản phẩm theo Category
- **Không bao gồm Topping** (theo yêu cầu)

---

## 🏗️ Kiến Trúc Clean Architecture

### Cấu Trúc Layers:

```
ProductService/
├── ProductService.API/          # Presentation Layer (REST + gRPC)
│   ├── Controllers/             # REST Controllers
│   ├── Services/                # gRPC Services
│   ├── Mapping/                 # gRPC Mapping Profiles
│   └── Protos/                  # gRPC Protocol Definitions
│
├── ProductService.Application/  # Application Layer (Business Logic)
│   ├── Services/                # Application Services
│   ├── DTOs/
│   │   ├── Requests/            # Request DTOs
│   │   └── Responses/           # Response DTOs
│   ├── Interfaces/               # Repository Interfaces
│   └── Mapping/                 # Domain -> DTO Mapping Profiles
│
├── ProductService.Domain/       # Domain Layer (Entities)
│   └── Entities/                # Domain Entities (Product, ProductVariant, Category)
│
└── ProductService.Infrastructure/ # Infrastructure Layer (Data Access)
    ├── Data/                    # DbContext
    └── Repositories/            # Repository Implementations
```

### Dependency Flow:
- **API** → **Application** → **Domain**
- **Infrastructure** → **Application** → **Domain**
- **API** → **Infrastructure** (chỉ cho DbContext registration)

---

## ⚙️ Cấu Hình Cần Thiết trong Program.cs

### 1. **gRPC Services**
```csharp
builder.Services.AddGrpc();
```
- Cho phép service hỗ trợ gRPC protocol
- Cần thiết cho gRPC endpoints

### 2. **REST API Controllers**
```csharp
builder.Services.AddControllers();
```
- Cho phép sử dụng REST API Controllers
- Cần thiết cho REST endpoints

### 3. **Swagger/OpenAPI Documentation**
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Product Service API",
        Version = "v1",
        Description = "API quản lý sản phẩm, size và giá"
    });
});
```
- Tạo API documentation tự động
- Chỉ hiển thị trong Development environment

### 4. **Database Context (PostgreSQL)**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(connectionString));
```
- Kết nối PostgreSQL database
- Connection string lấy từ `appsettings.json`

### 5. **Dependency Injection - Repositories**
```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```
- Đăng ký Repository pattern
- Lifetime: Scoped (một instance per HTTP request)

### 6. **Dependency Injection - Application Services**
```csharp
builder.Services.AddScoped<ProductApplicationService>();
```
- Đăng ký Application Service
- Lifetime: Scoped

### 7. **AutoMapper Configuration** ⚠️ **QUAN TRỌNG**
```csharp
using AutoMapper;
using ProductService.API.Mapping;
using ProductService.Application.Mapping;

// Configure AutoMapper
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ProductMappingProfile());        // Domain -> DTOs
    mc.AddProfile(new ProductGrpcMappingProfile());    // DTOs -> gRPC Messages
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
```
- **CẦN THIẾT** vì `ProductApplicationService` và `ProductGrpcService` đều sử dụng `IMapper`
- Lifetime: Singleton (một instance cho toàn bộ application)

### 8. **CORS Configuration**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```
- Cho phép cross-origin requests
- ⚠️ **Production**: Nên giới hạn origins cụ thể

### 9. **Middleware Pipeline**
```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
    });
}

app.UseCors();
app.UseAuthorization();
```

### 10. **Endpoint Mapping**
```csharp
app.MapControllers();                              // REST API endpoints
app.MapGrpcService<GreeterService>();             // gRPC Greeter (mẫu)
app.MapGrpcService<ProductGrpcService>();         // gRPC Product Service
app.MapGet("/", () => "Product Service API is running...");
```

---

## 🔌 Cấu Hình Port

### Trong `launchSettings.json`:
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5002"
    },
    "https": {
      "applicationUrl": "https://localhost:7077;http://localhost:5002"
    }
  }
}
```

### Trong `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5002"
      },
      "Https": {
        "Url": "https://localhost:7077"
      }
    },
    "EndpointDefaults": {
      "Protocols": "Http1AndHttp2"  // Cần Http2 cho gRPC
    }
  }
}
```

**Lưu ý:** 
- Port có thể được cấu hình ở 2 nơi:
  1. `launchSettings.json` - Chỉ áp dụng khi chạy từ Visual Studio hoặc `dotnet run` với profile
  2. `appsettings.json` - Áp dụng cho mọi môi trường, có thể override bằng environment variables
- Nếu muốn dùng port khác, có thể set environment variable: `ASPNETCORE_URLS=http://localhost:PORT`

### Cấu Hình Port Trong Program.cs (Tùy Chọn):
Nếu muốn cấu hình port trực tiếp trong code:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
    options.ListenLocalhost(7077, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps();
    });
});
```

**Hoặc sử dụng biến môi trường:**
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "5002";
builder.WebHost.UseUrls($"http://localhost:{port}");
```

---

## 🔄 Quy Trình Hoạt Động

### REST API Flow:

```
Client Request
    ↓
ProductsController (API Layer)
    ↓
ProductApplicationService (Application Layer)
    ↓ [Uses IMapper]
    ↓
IProductRepository (Interface - Application Layer)
    ↓
ProductRepository (Infrastructure Layer)
    ↓
ProductDbContext → PostgreSQL
    ↓
Domain Entities (Product, ProductVariant, Category)
    ↓ [Mapped by AutoMapper]
    ↓
ProductResponse DTOs
    ↓
JSON Response to Client
```

### gRPC Flow:

```
gRPC Client Request
    ↓
ProductGrpcService (API Layer)
    ↓ [Parse gRPC Request → Application Request DTO]
    ↓
ProductApplicationService (Application Layer)
    ↓ [Uses IMapper]
    ↓
IProductRepository → ProductRepository → Database
    ↓
Domain Entities
    ↓ [Mapped by AutoMapper]
    ↓
ProductResponse DTOs
    ↓ [Mapped by AutoMapper to gRPC Messages]
    ↓
gRPC Product/ProductSize Messages
    ↓
gRPC Response to Client
```

### Chi Tiết Mapping:

1. **Domain → Application DTO** (`ProductMappingProfile`):
   - `Product` → `ProductResponse`
   - `ProductVariant` → `ProductSizeResponse`
   - Xử lý null values và default values

2. **Application DTO → gRPC Message** (`ProductGrpcMappingProfile`):
   - `ProductResponse` → `Product` (gRPC)
   - `ProductSizeResponse` → `ProductSize` (gRPC)
   - Convert `Guid` → `string`, `decimal` → `double`

---

## 📦 Dependencies và Packages

### ProductService.API:
- `Grpc.AspNetCore` (v2.57.0) - gRPC support
- `Swashbuckle.AspNetCore` (v6.5.0) - Swagger/OpenAPI
- `Npgsql.EntityFrameworkCore.PostgreSQL` (v8.0.0) - PostgreSQL provider
- `Microsoft.EntityFrameworkCore.Design` (v8.0.0) - EF Core tools
- `AutoMapper` (v13.0.1) - Object mapping

### ProductService.Application:
- `AutoMapper` (v13.0.1) - Object mapping

### ProductService.Infrastructure:
- `Npgsql.EntityFrameworkCore.PostgreSQL` (v8.0.0) - PostgreSQL provider
- `Microsoft.EntityFrameworkCore.Design` (v8.0.0) - EF Core tools

### ProductService.Domain:
- Không có external dependencies (Pure Domain)

---

## 🌐 API Endpoints

### REST API:

#### 1. GET /api/products
- **Query Parameters:**
  - `categoryId` (Guid?, optional) - Lọc theo Category
- **Response:** `ProductListResponse`
  ```json
  {
    "products": [
      {
        "id": "guid",
        "name": "string",
        "description": "string",
        "imageUrl": "string",
        "isActive": true,
        "categoryId": "guid",
        "categoryName": "string",
        "sizes": [
          {
            "id": "guid",
            "sku": "string",
            "name": "string",
            "attributes": "string",
            "basePrice": 0.00,
            "isActive": true
          }
        ]
      }
    ],
    "totalCount": 0
  }
  ```

#### 2. GET /api/products/{id}
- **Path Parameters:**
  - `id` (Guid) - Product ID
- **Response:** `ProductResponse` hoặc 404 Not Found

### gRPC API:

#### 1. GetProducts
- **Request:** `GetProductsRequest`
  - `category_id` (string, optional)
- **Response:** `GetProductsReply`
  - `products` (repeated Product)
  - `total_count` (int32)

#### 2. GetProductById
- **Request:** `GetProductByIdRequest`
  - `id` (string, required)
- **Response:** `GetProductByIdReply`
  - `product` (Product)

### Swagger UI:
- **Development:** `http://localhost:5002/swagger` hoặc `https://localhost:7077/swagger`

---

## ✅ Checklist Setup

- [ ] PostgreSQL database đã được tạo và chạy
- [ ] Connection string trong `appsettings.json` đúng
- [ ] AutoMapper đã được đăng ký trong `Program.cs`
- [ ] Tất cả services và repositories đã được đăng ký DI
- [ ] Port configuration trong `launchSettings.json` hoặc `appsettings.json`
- [ ] Kestrel protocols = `Http1AndHttp2` (cho gRPC)
- [ ] CORS đã được cấu hình (nếu cần)
- [ ] Swagger chỉ bật trong Development

---

## 🚀 Chạy Service

```bash
# Development
dotnet run --project ProductService/ProductService.API

# Hoặc với profile cụ thể
dotnet run --project ProductService/ProductService.API --launch-profile http
dotnet run --project ProductService/ProductService.API --launch-profile https
```

**Service sẽ chạy tại:**
- HTTP: `http://localhost:5002`
- HTTPS: `https://localhost:7077`
- Swagger: `http://localhost:5002/swagger`

---

## 📝 Lưu Ý Quan Trọng

1. **AutoMapper là BẮT BUỘC** - Nếu không đăng ký, service sẽ lỗi vì `ProductApplicationService` và `ProductGrpcService` đều inject `IMapper`

2. **gRPC cần HTTP/2** - Đảm bảo `Kestrel.EndpointDefaults.Protocols = "Http1AndHttp2"`

3. **Database Migration** - Nhớ chạy migrations trước khi start service:
   ```bash
   dotnet ef database update --project ProductService/ProductService.Infrastructure --startup-project ProductService/ProductService.API
   ```

4. **CORS trong Production** - Nên giới hạn `AllowAnyOrigin()` thành specific origins

5. **Connection String** - Không commit connection string thật vào git, dùng User Secrets hoặc Environment Variables

---

## 🔍 Troubleshooting

### Lỗi: "Cannot resolve service for type 'AutoMapper.IMapper'"
**Giải pháp:** Thêm AutoMapper registration vào `Program.cs` (xem mục 7 ở trên)

### Lỗi: "gRPC requires HTTP/2"
**Giải pháp:** Đảm bảo `appsettings.json` có `Kestrel.EndpointDefaults.Protocols = "Http1AndHttp2"`

### Lỗi: "Connection refused" hoặc database connection failed
**Giải pháp:** 
- Kiểm tra PostgreSQL đang chạy
- Kiểm tra connection string trong `appsettings.json`
- Kiểm tra firewall/network

---

**Tài liệu này được cập nhật lần cuối:** [Ngày hiện tại]
