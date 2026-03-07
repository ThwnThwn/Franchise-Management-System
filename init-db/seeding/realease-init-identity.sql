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
    u_staff_ids UUID[] := ARRAY['99999999-9999-9999-9999-999999999999', '88888888-8888-8888-8888-888888888888', '77777777-7777-7777-7777-777777777777', '66666666-6666-6666-6666-666666666666'];
    
    -- 15 ID mẫu cho Customer
    cust_sample_ids UUID[] := ARRAY[
        '00964234-2ce1-4bbd-b926-64bf68fef73b', '01549a30-76eb-4148-9cae-97eb31b853d4',
        '07204f59-80da-4c1c-8a8f-462300a003de', '0d03e6fd-ae82-4183-aacd-707930a134fd',
        '134ff6db-eb5a-4bde-a92c-8faf8ffb37bf', '1ded7707-40d6-4467-997a-abeaf70d9470',
        '1ea54879-27d0-4b26-a0bf-60cb9b4f768a', '01feadb5-f1fc-45e9-8360-6fbdeea929ae',
        '24a66acc-f2cd-4e58-9b47-f17e19df80aa', '26c7f49d-c1ef-4f65-bab3-71f12f42e51c',
        '2a9dd74f-3284-4a0b-8cfc-0db5dbd43664', '2b9b0400-f37e-411a-ab97-7c58876182d6',
        '2e4194b0-649a-4103-ab63-15e5cc5cb92b', '3014aef9-f746-443f-85d8-a700185278db',
        '3707976c-40cf-4525-82af-88d22d23f56f'
    ];
    
    pass_hash VARCHAR := '$2a$12$mbTUUQ8yfeZt9G736FQdbeBTfVY6CesLBxhLCuEYkkb6C96f4Hdj2';
    i INT;
BEGIN
    -- 1. Admin
    INSERT INTO users (first_name, last_name, email, username, password, role_id, date_of_birth)
    VALUES ('Super', 'Admin', 'admin@cahongban.com', 'okbanlanhat', pass_hash, r_admin, '1990-01-01')
    ON CONFLICT DO NOTHING;

    -- 2. 10 Managers
    FOR i IN 1..10 LOOP
        INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id)
        VALUES (
            ('f0000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0'))::UUID,
            'Manager', i, 'manager' || i || '@cahongban.com', 'mana' || LPAD(i::text, 2, '0'), 
            pass_hash, r_manager, '1992-05-15', s_ids[(i-1) % 3 + 1]
        ) ON CONFLICT DO NOTHING;
    END LOOP;

    -- 3. 19 Staff
    FOR i IN 1..19 LOOP
        IF i <= 4 THEN
            INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id)
            VALUES (u_staff_ids[i], 'Staff Spec', i, 'staff' || i || '@cahongban.com', 'staff' || LPAD(i::text, 2, '0'), 
            pass_hash, r_staff, '1998-01-01', s_ids[(i-1) % 3 + 1]) ON CONFLICT DO NOTHING;
        ELSE
            INSERT INTO users (first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id)
            VALUES ('Staff', i, 'staff' || i || '@cahongban.com', 'staff' || LPAD(i::text, 2, '0'), 
            pass_hash, r_staff, '1999-01-01', s_ids[(i-1) % 3 + 1]) ON CONFLICT DO NOTHING;
        END IF;
    END LOOP;

    -- 4. 15 Customers (ID cố định)
    FOR i IN 1..15 LOOP
        INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth)
        VALUES (cust_sample_ids[i], 'Customer', i, 'cust' || i || '@gmail.com', 'cust' || LPAD(i::text, 2, '0'), 
        pass_hash, r_customer, '2005-05-05') ON CONFLICT DO NOTHING;
    END LOOP;

    -- 5. 55 Customers (ID ngẫu nhiên)
    FOR i IN 16..70 LOOP
        INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth)
        VALUES (uuid_generate_v4(), 'Customer', i, 'cust' || i || '@gmail.com', 'cust' || LPAD(i::text, 2, '0'), 
        pass_hash, r_customer, '2005-05-05') ON CONFLICT DO NOTHING;
    END LOOP;

END $$;