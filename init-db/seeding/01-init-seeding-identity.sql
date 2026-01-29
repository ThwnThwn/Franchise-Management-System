-- ================================================================
-- FILE 2: SEED DATA CHO IDENTITY SERVICE (UPDATED SCHEMA)
-- ================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. SEED ROLES (Nếu chưa có)
INSERT INTO roles (name, slug, description) VALUES 
('Administrator', 'admin', 'Quản trị viên hệ thống'),
('Manager', 'manager', 'Cửa hàng trưởng'),
('Staff', 'staff', 'Nhân viên bán hàng/Pha chế'),
('Customer', 'customer', 'Khách hàng thân thiết')
ON CONFLICT (slug) DO NOTHING;

DO $$
DECLARE
    -- === COPY UUID TỪ FRANCHISE DB SANG ===
    -- Stores
    s_q1 UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd'; 
    s_q3 UUID := 'e1488c32-1e33-498f-9130-9e13f841c8ea'; 
    s_td UUID := 'da4aa30d-be1e-4e1e-874a-feaec06c6169'; 
    
    -- Users (Phải khớp với bảng Schedules bên kia)
    u_staff_q1 UUID := '99999999-9999-9999-9999-999999999999';
    u_staff_q3 UUID := '88888888-8888-8888-8888-888888888888';
    u_staff_td UUID := '77777777-7777-7777-7777-777777777777';

    -- Role IDs (Lấy từ DB lên)
    r_admin UUID; r_manager UUID; r_staff UUID; r_customer UUID;
    
    -- Pass mặc định (123456)
    default_pass VARCHAR := '$2a$12$mbTUUQ8yfeZt9G736FQdbeBTfVY6CesLBxhLCuEYkkb6C96f4Hdj2'; 
    i INT;
BEGIN
    -- Lấy ID Role
    SELECT id INTO r_admin FROM roles WHERE slug = 'admin';
    SELECT id INTO r_manager FROM roles WHERE slug = 'manager';
    SELECT id INTO r_staff FROM roles WHERE slug = 'staff';
    SELECT id INTO r_customer FROM roles WHERE slug = 'customer';

    -- === 1. TẠO ADMIN (Không thuộc Store nào) ===
    INSERT INTO users (
        first_name, last_name, email, username, password, phone, address, 
        gender, date_of_birth, role_id, home_store_id
    )
    VALUES (
        'Super', 'Admin', 'admin@email.com', 'admin', default_pass, '0909000001', 'Head Office', 
        0, '1990-01-01 00:00:00', r_admin, NULL
    );

    -- === 2. TẠO MANAGERS ===
    -- Manager Q1
    INSERT INTO users (
        first_name, last_name, email, username, password, phone, address, 
        gender, date_of_birth, role_id, home_store_id
    )
    VALUES (
        'Manager', 'Quan 1', 'manager1@email.com', 'manager1', default_pass, '0910000001', 'District 1', 
        0, '1992-05-15 00:00:00', r_manager, s_q1
    );

    -- Manager Q3
    INSERT INTO users (
        first_name, last_name, email, username, password, phone, address, 
        gender, date_of_birth, role_id, home_store_id
    )
    VALUES (
        'Manager', 'Quan 3', 'manager2@email.com', 'manager2', default_pass, '0910000002', 'District 3', 
        1, '1993-06-20 00:00:00', r_manager, s_q3
    );

    -- === 3. TẠO 3 STAFF ĐẶC BIỆT (Có lịch làm việc) ===
    
    -- Staff Q1 (Username: staff_q1)
    INSERT INTO users (
        id, first_name, last_name, email, username, password, phone, address,
        gender, date_of_birth, role_id, home_store_id
    )
    VALUES (
        u_staff_q1, 'Nhan Vien', 'Quan 1', 'staff_q1@email.com', 'staff_q1', default_pass, '0928880001', 'Staff House 1',
        0, '1998-01-01 00:00:00', r_staff, s_q1
    );

    -- Staff Q3 (Username: staff_q3)
    INSERT INTO users (
        id, first_name, last_name, email, username, password, phone, address,
        gender, date_of_birth, role_id, home_store_id
    )
    VALUES (
        u_staff_q3, 'Nhan Vien', 'Quan 3', 'staff_q3@email.com', 'staff_q3', default_pass, '0928880003', 'Staff House 3',
        1, '1999-02-02 00:00:00', r_staff, s_q3
    );

    -- Staff Thủ Đức (Username: staff_td)
    INSERT INTO users (
        id, first_name, last_name, email, username, password, phone, address,
        gender, date_of_birth, role_id, home_store_id
    )
    VALUES (
        u_staff_td, 'Nhan Vien', 'Thu Duc', 'staff_td@email.com', 'staff_td', default_pass, '0928880004', 'Staff House TD',
        0, '2000-03-03 00:00:00', r_staff, s_td
    );

    -- === 4. TẠO STAFF PHỤ (Random - Để test pagination) ===
    FOR i IN 1..3 LOOP
        INSERT INTO users (
            first_name, last_name, email, username, password, phone, address,
            gender, date_of_birth, role_id, home_store_id
        )
        VALUES (
            'Staff', 'Extra ' || i, 'staff_extra' || i || '@email.com', 'staff_extra' || i, default_pass, '09200000' || i, 'Dorm',
            (i % 2), '2001-01-01 00:00:00', r_staff, s_q1
        );
    END LOOP;

    -- === 5. TẠO CUSTOMER ===
    FOR i IN 1..10 LOOP
        INSERT INTO users (
            first_name, last_name, email, username, password, phone, address,
            gender, date_of_birth, role_id, home_store_id
        )
        VALUES (
            'Customer', 'Mem ' || i, 'cust' || i || '@email.com', 'cust' || i, default_pass, '09300000' || i, 'Street ' || i,
            (i % 2), '2005-05-05 00:00:00', r_customer, NULL
        );
    END LOOP;

    RAISE NOTICE 'Identity DB Seeded Successfully!';
END $$;