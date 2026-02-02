-- IDENTITY SERVICE SEED
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
TRUNCATE refresh_tokens, users, roles CASCADE;

-- 1. SEED ROLES
INSERT INTO roles (id, name, slug, description) VALUES 
('e2000000-0000-0000-0000-000000000001', 'Administrator', 'admin', 'Quản trị viên hệ thống'),
('e2000000-0000-0000-0000-000000000002', 'Manager', 'manager', 'Cửa hàng trưởng'),
('e2000000-0000-0000-0000-000000000003', 'Staff', 'staff', 'Nhân viên bán hàng/Pha chế'),
('e2000000-0000-0000-0000-000000000004', 'Customer', 'customer', 'Khách hàng thân thiết');

DO $$
DECLARE
    r_admin UUID := 'e2000000-0000-0000-0000-000000000001';
    r_manager UUID := 'e2000000-0000-0000-0000-000000000002';
    r_staff UUID := 'e2000000-0000-0000-0000-000000000003';
    r_customer UUID := 'e2000000-0000-0000-0000-000000000004';
    s_ids UUID[] := ARRAY['b7ac8328-2ca3-470d-80ca-15444eeab2dd', 'e1488c32-1e33-498f-9130-9e13f841c8ea', 'da4aa30d-be1e-4e1e-874a-feaec06c6169'];
    u_staff_ids UUID[] := ARRAY['99999999-9999-9999-9999-999999999999', '88888888-8888-8888-8888-888888888888', '77777777-7777-7777-7777-777777777777'];
    pass_hash VARCHAR := '$2a$12$mbTUUQ8yfeZt9G736FQdbeBTfVY6CesLBxhLCuEYkkb6C96f4Hdj2';
    i INT;
BEGIN
    -- 1. Admin
    INSERT INTO users (first_name, last_name, email, username, password, role_id, date_of_birth)
    VALUES ('Super', 'Admin', 'admin@cahongban.com', 'okbanlanhat', pass_hash, r_admin, '1990-01-01');

    -- 2. 10 Managers (ID cố định f000...)
    FOR i IN 1..10 LOOP
        INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id)
        VALUES (
            ('f0000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0'))::UUID,
            'Manager', i, 'manager' || i || '@cahongban.com', 'mana' || LPAD(i::text, 2, '0'), 
            pass_hash, r_manager, '1992-05-15', s_ids[(i-1) % 3 + 1]
        );
    END LOOP;

    -- 3. 19 Staff
    FOR i IN 1..19 LOOP
        IF i <= 3 THEN
            INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id)
            VALUES (u_staff_ids[i], 'Staff Spec', i, 'staff' || i || '@cahongban.com', 'staff' || LPAD(i::text, 2, '0'), 
            pass_hash, r_staff, '1998-01-01', s_ids[i]);
        ELSE
            INSERT INTO users (first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id)
            VALUES ('Staff', i, 'staff' || i || '@cahongban.com', 'staff' || LPAD(i::text, 2, '0'), 
            pass_hash, r_staff, '1999-01-01', s_ids[(i-1) % 3 + 1]);
        END IF;
    END LOOP;

    -- 4. 70 Customers
    FOR i IN 1..70 LOOP
        INSERT INTO users (first_name, last_name, email, username, password, role_id, date_of_birth)
        VALUES ('Customer', i, 'cust' || i || '@gmail.com', 'cust' || LPAD(i::text, 2, '0'), pass_hash, r_customer, '2005-05-05');
    END LOOP;
END $$;