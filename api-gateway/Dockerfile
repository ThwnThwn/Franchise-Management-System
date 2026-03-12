# ==========================================
# STAGE 1: BUILD (Chốt đúng Go 1.24.5)
# ==========================================
FROM golang:1.24.5-alpine AS build
WORKDIR /app

# 1. Copy file quản lý thư viện và tải về trước
COPY go.mod go.sum ./
RUN go mod download

# 2. Copy toàn bộ source code Go vào
COPY . .

# 3. Build ra file nhị phân (Binary) cho Linux
# Trỏ chính xác vào đường dẫn ./cmd/server/main.go thay vì dấu chấm (.)
# Output sẽ được lưu vào /app/bin/main (không có đuôi .exe)
RUN CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -o /app/bin/main ./cmd/server/main.go

# ==========================================
# STAGE 2: RUNTIME 
# ==========================================
FROM alpine:latest AS final
WORKDIR /app

# Cài chứng chỉ SSL để Gateway gọi được API HTTPS ra ngoài (VNPAY, v.v.)
RUN apk --no-cache add ca-certificates

# Copy file binary Linux từ Stage 1 sang Stage 2
COPY --from=build /app/bin/main .

# Cấp quyền thực thi cho file (Tránh lỗi Permission Denied)
RUN chmod +x ./main

# Mở cổng 8080
EXPOSE 8080

# Chạy app (gọi file Linux binary)
ENTRYPOINT ["./main"]