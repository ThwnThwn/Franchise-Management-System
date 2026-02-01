-- ================================================================
-- FILE 1: SEED DATA CHO FRANCHISE SERVICE (UPDATED SCHEMA)
-- ================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Dọn dẹp dữ liệu cũ
TRUNCATE TABLE time_logs, schedules, shift_templates, store_hours, stores, franchisees CASCADE;

DO $$
DECLARE
    -- === 1. KHAI BÁO UUID CỐ ĐỊNH (MASTER DATA) ===
    
    -- Franchisees
    f_hq UUID := '11111111-1111-1111-1111-111111111111';
    f_partner UUID := '22222222-2222-2222-2222-222222222222';

    -- Stores (Dùng để map sang Identity.home_store_id)
    s_q1 UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd'; -- Store Quận 1
    s_q3 UUID := 'e1488c32-1e33-498f-9130-9e13f841c8ea'; -- Store Quận 3
    s_td UUID := 'da4aa30d-be1e-4e1e-874a-feaec06c6169'; -- Store Thủ Đức

    -- Shift Templates
    tpl_morning UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    tpl_evening UUID := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';

    -- Users (Staff) có lịch làm việc (Dùng để map sang Identity.Users.id)
    u_staff_q1 UUID := '99999999-9999-9999-9999-999999999999'; -- Staff Q1 (Ca Sáng)
    u_staff_q3 UUID := '88888888-8888-8888-8888-888888888888'; -- Staff Q3 (Ca Sáng)
    u_staff_td UUID := '77777777-7777-7777-7777-777777777777'; -- Staff Thủ Đức (Ca Chiều)

    -- Biến phụ trợ
    i INT;
    cur_date DATE := CURRENT_DATE;
BEGIN
    -- === 2. SEED FRANCHISEES ===
    INSERT INTO franchisees (id, name, tax_code, representative_name, phone, email, commission_rate)
    VALUES (f_hq, 'Coffee Chain HQ', '0101234567', 'Nguyen Van CEO', '02839999999', 'contact@hq.com', 0);

    INSERT INTO franchisees (id, name, tax_code, representative_name, phone, email, commission_rate)
    VALUES (f_partner, 'Nguyen Van A Partners', '0307654321', 'Nguyen Van A', '0909123456', 'partner.a@gmail.com', 5.0);

    -- === 3. SEED STORES ===
    INSERT INTO stores (id, code, name, address, phone, email, latitude, longitude, type, status, franchisee_id)
    VALUES (s_q1, 'STR-Q1-001', 'Coffee Central Q1', '1 Cong Xa Paris, Dist 1, HCM', '02811112222', 'store.q1@hq.com', 10.779785, 106.699018, 0, 1, f_hq);

    INSERT INTO stores (id, code, name, address, phone, email, latitude, longitude, type, status, franchisee_id)
    VALUES (s_q3, 'STR-Q3-002', 'Coffee Lake Q3', '45 Pham Ngoc Thach, Dist 3, HCM', '02833334444', 'store.q3@hq.com', 10.782877, 106.695847, 0, 1, f_hq);

    INSERT INTO stores (id, code, name, address, phone, email, latitude, longitude, type, status, franchisee_id)
    VALUES (s_td, 'STR-TD-003', 'Coffee Giga TD', '242 Pham Van Dong, Thu Duc, HCM', '02855556666', 'store.td@partner.com', 10.827641, 106.721458, 1, 1, f_partner);

    -- === 4. SEED STORE HOURS ===
    FOR i IN 0..6 LOOP
        INSERT INTO store_hours (store_id, day_of_week, open_time, close_time) VALUES (s_q1, i, '07:00', '22:00');
        INSERT INTO store_hours (store_id, day_of_week, open_time, close_time) VALUES (s_q3, i, '07:00', '22:00');
        INSERT INTO store_hours (store_id, day_of_week, open_time, close_time) VALUES (s_td, i, '07:30', '22:30');
    END LOOP;

    -- === 5. SEED SHIFT TEMPLATES ===
    INSERT INTO shift_templates (id, store_id, code, name, start_time, end_time, break_duration_minutes)
    VALUES (tpl_morning, NULL, 'MORNING', 'Ca Sáng Tiêu Chuẩn', '07:00', '15:00', 30); -- NULL store_id = áp dụng toàn hệ thống

    INSERT INTO shift_templates (id, store_id, code, name, start_time, end_time, break_duration_minutes)
    VALUES (tpl_evening, NULL, 'EVENING', 'Ca Chiều Tiêu Chuẩn', '14:00', '22:00', 30);

    -- === 6. SEED SCHEDULES (Gắn lịch cho 3 staff cụ thể) ===
    
    -- Staff Q1: Ca Sáng
    INSERT INTO schedules (user_id, store_id, shift_template_id, shift_date, start_time, end_time, status, hourly_wage)
    VALUES (u_staff_q1, s_q1, tpl_morning, cur_date, 
        (cur_date || ' 07:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        (cur_date || ' 15:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        1, 25000);

    -- Staff Q3: Ca Sáng
    INSERT INTO schedules (user_id, store_id, shift_template_id, shift_date, start_time, end_time, status, hourly_wage)
    VALUES (u_staff_q3, s_q3, tpl_morning, cur_date, 
        (cur_date || ' 07:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        (cur_date || ' 15:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        1, 25000);

    -- Staff Thủ Đức: Ca Chiều
    INSERT INTO schedules (user_id, store_id, shift_template_id, shift_date, start_time, end_time, status, hourly_wage)
    VALUES (u_staff_td, s_td, tpl_evening, cur_date, 
        (cur_date || ' 14:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        (cur_date || ' 22:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        1, 28000);

    RAISE NOTICE 'Franchise DB Seeded Successfully!';
END $$;