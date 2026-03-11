-- Kích hoạt extension nếu DB wallet chưa có
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

INSERT INTO wallets (
    id,
    user_id, 
    balance, 
    status, 
    notes, 
    row_version, 
    updated_by
) VALUES (
    uuid_generate_v4(),                               -- ID tự động sinh cho ví
    '00964234-2ce1-4bbd-b926-64bf68fef73b',           -- ID chuẩn của cust01
    500000.00,                                        -- Cho sẵn 500k để test thanh toán
    1,                                                -- Status: 1 (Giả định 1 là Active/Hoạt động)
    'Ví khởi tạo mặc định cho cust01',
    '\x0000000000000000'::bytea,                      -- Khởi tạo row_version (dành cho Optimistic Concurrency)
    'e2000000-0000-0000-0000-000000000001'            -- updated_by: lấy ID của Super Admin
);