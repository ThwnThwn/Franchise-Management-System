# Product Service API - GET /products

## Mô tả
API trả về danh sách sản phẩm kèm theo thông tin **Size** (Product Variant) và **Giá** (Price).

## Endpoints

### 1. GET /api/products
Lấy danh sách tất cả sản phẩm.

**Query Parameters:**
- `categoryId` (optional, UUID): Lọc sản phẩm theo Category ID

**Response Example:**
```json
{
  "products": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Cappuccino",
      "description": "Cà phê cappuccino thơm ngon",
      "imageUrl": "https://example.com/cappuccino.jpg",
      "isActive": true,
      "categoryId": "660e8400-e29b-41d4-a716-446655440000",
      "categoryName": "Coffee",
      "sizes": [
        {
          "id": "770e8400-e29b-41d4-a716-446655440000",
          "sku": "CAP-S",
          "name": "Small",
          "attributes": "{\"size\": \"S\"}",
          "basePrice": 35000,
          "isActive": true
        },
        {
          "id": "880e8400-e29b-41d4-a716-446655440000",
          "sku": "CAP-M",
          "name": "Medium",
          "attributes": "{\"size\": \"M\"}",
          "basePrice": 45000,
          "isActive": true
        },
        {
          "id": "990e8400-e29b-41d4-a716-446655440000",
          "sku": "CAP-L",
          "name": "Large",
          "attributes": "{\"size\": \"L\"}",
          "basePrice": 55000,
          "isActive": true
        }
      ]
    }
  ],
  "totalCount": 1
}
```

### 2. GET /api/products/{id}
Lấy thông tin chi tiết một sản phẩm theo ID.

**Path Parameters:**
- `id` (required, UUID): Product ID

**Response:** Giống như một phần tử trong mảng `products` ở endpoint GET /api/products

## Cách sử dụng

### 1. Lấy tất cả sản phẩm:
```bash
GET http://localhost:5002/api/products
```

### 2. Lọc sản phẩm theo Category:
```bash
GET http://localhost:5002/api/products?categoryId=660e8400-e29b-41d4-a716-446655440000
```

### 3. Lấy thông tin một sản phẩm:
```bash
GET http://localhost:5002/api/products/550e8400-e29b-41d4-a716-446655440000
```

## Swagger Documentation
Khi chạy ở Development mode, bạn có thể truy cập Swagger UI tại:
```
http://localhost:5002/swagger
```

## Database Schema

### Tables Used:
1. **products** - Sản phẩm
2. **product_variants** - Size và giá của sản phẩm  
3. **categories** - Phân loại sản phẩm

## Cấu trúc dữ liệu trả về

- **Product**: Thông tin sản phẩm chính
  - `sizes`: Danh sách các size/variant với giá (basePrice)
  
Mỗi size có giá riêng (basePrice) tương ứng.

## Chạy API

```bash
cd c:\OJT\chbms\chb-cfms-be\backend\ProductService\ProductService.API
dotnet run
```

API sẽ chạy ở: `http://localhost:5002`
