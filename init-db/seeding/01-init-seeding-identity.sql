-- 1. Kích hoạt Extension UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. SEED ROLES (Nếu chưa có)
INSERT INTO roles (name, slug, description) VALUES 
('Administrator', 'admin', 'Quản trị viên hệ thống'),
('Manager', 'manager', 'Cửa hàng trưởng'),
('Staff', 'staff', 'Nhân viên bán hàng/Pha chế'),
('Customer', 'customer', 'Khách hàng thân thiết')
ON CONFLICT (slug) DO NOTHING;

-- 3. SEED USERS VỚI LOGIC PHÂN QUYỀN & CỬA HÀNG
DO $$
DECLARE
    -- Khai báo biến giữ ID của Role
    r_admin UUID;
    r_manager UUID;
    r_staff UUID;
    r_customer UUID;

    -- Khai báo biến giả lập ID Cửa hàng (Vì Store nằm ở DB khác)
    store_q1 UUID := uuid_generate_v4(); -- Cửa hàng Quận 1
    store_q3 UUID := uuid_generate_v4(); -- Cửa hàng Quận 3
    store_td UUID := uuid_generate_v4(); -- Cửa hàng Thủ Đức
    
    -- Biến chạy vòng lặp
    i INT;
    
    -- Password hash mặc định cho '123456'
    -- (Lưu ý: Nếu không login được, hãy thay bằng hash từ code của bạn)
    default_pass VARCHAR := '$2a$12$mbTUUQ8yfeZt9G736FQdbeBTfVY6CesLBxhLCuEYkkb6C96f4Hdj2'; 
BEGIN
    -- Lấy ID của các Role từ bảng roles
    SELECT id INTO r_admin FROM roles WHERE slug = 'admin';
    SELECT id INTO r_manager FROM roles WHERE slug = 'manager';
    SELECT id INTO r_staff FROM roles WHERE slug = 'staff';
    SELECT id INTO r_customer FROM roles WHERE slug = 'customer';

    -- =================================================
    -- LOOP 1: TẠO 1 ADMIN (admin1@email.com)
    -- Home Store: NULL (Làm tại trụ sở)
    -- =================================================
    FOR i IN 1..1 LOOP
        INSERT INTO users (
            first_name, last_name, email, username, password, phone, address, 
            gender, date_of_birth, role_id, home_store_id
        )
        VALUES (
            'Super', 'Admin', 
            'admin' || i || '@email.com', 
            'admin' || i,
            default_pass, 
            '0909000001', 'Head Office, HCM', 1, '1990-01-01', r_admin, 
            NULL -- Admin không thuộc cửa hàng cụ thể
        );
    END LOOP;

    -- =================================================
    -- LOOP 2: TẠO 2 MANAGER (manager1, manager2)
    -- Phân bổ: Manager 1 quản lý Quận 1, Manager 2 quản lý Quận 3
    -- =================================================
    FOR i IN 1..2 LOOP
        INSERT INTO users (
            first_name, last_name, email, username, password, phone, address, 
            gender, date_of_birth, role_id, home_store_id
        )
        VALUES (
            'Manager', 'Store ' || i, 
            'manager' || i || '@email.com', 
            'manager' || i,
            default_pass, 
            '091000000' || i, 'Manager House', 1, '1992-05-15', r_manager,
            CASE 
                WHEN i = 1 THEN store_q1 
                ELSE store_q3 
            END
        );
    END LOOP;

    -- =================================================
    -- LOOP 3: TẠO 5 STAFF (staff1 -> staff5)
    -- Phân bổ: Rải đều vào 3 cửa hàng (Q1, Q3, Thủ Đức)
    -- =================================================
    FOR i IN 1..5 LOOP
        INSERT INTO users (
            first_name, last_name, email, username, password, phone, address, 
            gender, date_of_birth, role_id, home_store_id
        )
        VALUES (
            'Staff', 'Employee ' || i, 
            'staff' || i || '@email.com', 
            'staff' || i,
            default_pass, 
            '092000000' || i, 'Staff Dorm', 0, '1998-08-20', r_staff,
            CASE 
                WHEN (i % 3) = 0 THEN store_q1 
                WHEN (i % 3) = 1 THEN store_q3 
                ELSE store_td 
            END
        );
    END LOOP;

    -- =================================================
    -- LOOP 4: TẠO 92 CUSTOMER (customer1 -> customer92)
    -- Home Store: NULL (Khách hàng không biên chế)
    -- =================================================
    FOR i IN 1..92 LOOP
        INSERT INTO users (
            first_name, last_name, email, username, password, phone, address, 
            gender, date_of_birth, role_id, home_store_id
        )
        VALUES (
            'Customer', 'Member ' || i, 
            'customer' || i || '@email.com', 
            'customer' || i,
            default_pass, 
            '093' || LPAD(i::text, 7, '0'), 'Street ' || i, (i % 2), 
            NOW() - (interval '18 years' + (i || ' days')::interval), -- Random tuổi > 18
            r_customer,
            NULL -- Khách hàng
        );
    END LOOP;

END $$;