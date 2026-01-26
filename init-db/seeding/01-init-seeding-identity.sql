-- 1. Bật Extension UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Đảm bảo bảng Roles có dữ liệu (Nếu chưa có)
INSERT INTO roles (name, slug, description) VALUES 
('Administrator', 'admin', 'Quản trị viên'),
('Manager', 'manager', 'Quản lý'),
('Staff', 'staff', 'Nhân viên'),
('Customer', 'customer', 'Khách hàng')
ON CONFLICT (slug) DO NOTHING;

-- 3. CHẠY LOGIC TẠO 100 USER
DO $$
DECLARE
    -- Khai báo biến ID của các Role
    r_admin UUID;
    r_manager UUID;
    r_staff UUID;
    r_customer UUID;
    
    -- Biến chạy vòng lặp
    i INT;
    
    -- Password hash (123456)
    default_pass VARCHAR := '$2a$12$mbTUUQ8yfeZt9G736FQdbeBTfVY6CesLBxhLCuEYkkb6C96f4Hdj2'; 
BEGIN
    -- Lấy ID của từng Role gán vào biến
    SELECT id INTO r_admin FROM roles WHERE slug = 'admin';
    SELECT id INTO r_manager FROM roles WHERE slug = 'manager';
    SELECT id INTO r_staff FROM roles WHERE slug = 'staff';
    SELECT id INTO r_customer FROM roles WHERE slug = 'customer';

    -- =================================================
    -- LOOP 1: TẠO 1 ADMIN (admin1@email.com)
    -- =================================================
    FOR i IN 1..1 LOOP
        INSERT INTO users (first_name, last_name, email, username, password, phone, address, gender, date_or_birth, role_id)
        VALUES (
            'Admin', 'System', 
            'admin' || i || '@email.com',  -- admin1@email.com
            'admin' || i,
            default_pass, 
            '0909000001', 'Admin HQ', 1, '1990-01-01', r_admin
        );
    END LOOP;

    -- =================================================
    -- LOOP 2: TẠO 2 MANAGERS (manager1 -> manager2)
    -- =================================================
    FOR i IN 1..2 LOOP
        INSERT INTO users (first_name, last_name, email, username, password, phone, address, gender, date_or_birth, role_id)
        VALUES (
            'Manager', 'Num ' || i, 
            'manager' || i || '@email.com', -- manager1@email.com
            'manager' || i,
            default_pass, 
            '091000000' || i, 'Manager Room', 1, '1992-01-01', r_manager
        );
    END LOOP;

    -- =================================================
    -- LOOP 3: TẠO 5 STAFFS (staff1 -> staff5)
    -- =================================================
    FOR i IN 1..5 LOOP
        INSERT INTO users (first_name, last_name, email, username, password, phone, address, gender, date_or_birth, role_id)
        VALUES (
            'Staff', 'Num ' || i, 
            'staff' || i || '@email.com',   -- staff1@email.com
            'staff' || i,
            default_pass, 
            '092000000' || i, 'Staff Counter', 0, '1995-05-05', r_staff
        );
    END LOOP;

    -- =================================================
    -- LOOP 4: TẠO 92 CUSTOMERS (customer1 -> customer92)
    -- =================================================
    FOR i IN 1..92 LOOP
        INSERT INTO users (first_name, last_name, email, username, password, phone, address, gender, date_or_birth, role_id)
        VALUES (
            'Customer', 'Num ' || i, 
            'customer' || i || '@email.com', -- customer1@email.com
            'customer' || i,
            default_pass, 
            '093' || LPAD(i::text, 7, '0'), 'Street ' || i, (i % 2), 
            NOW() - (interval '18 years' + (i || ' days')::interval), 
            r_customer
        );
    END LOOP;

END $$;