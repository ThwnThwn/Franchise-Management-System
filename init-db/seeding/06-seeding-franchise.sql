-- 1. Extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Dọn dẹp dữ liệu cũ (nếu muốn reset sạch sẽ thì bỏ comment dòng dưới)
-- TRUNCATE TABLE time_logs, schedules, shift_templates, store_hours, stores, franchisees CASCADE;

-- 3. Bắt đầu Seeding
DO $$
DECLARE
    -- ID Franchisee
    f_hq UUID;
    f_partner UUID;

    -- ID Stores
    s_q1 UUID;
    s_q3 UUID;
    s_td UUID;

    -- ID Templates
    tpl_morning UUID;
    tpl_evening UUID;

    -- Biến chạy
    i INT;
    cur_date DATE := CURRENT_DATE;
BEGIN
    -- =================================================
    -- 1. SEED FRANCHISEES (Đối tác)
    -- =================================================
    
    -- Cty Mẹ (Sở hữu chuỗi)
    INSERT INTO franchisees (name, tax_code, representative_name, phone, email, commission_rate)
    VALUES ('Coffee Chain HQ', '0101234567', 'Nguyen Van CEO', '02839999999', 'contact@hq.com', 0)
    RETURNING id INTO f_hq;

    -- Đối tác nhượng quyền (Mr. A)
    INSERT INTO franchisees (name, tax_code, representative_name, phone, email, commission_rate)
    VALUES ('Nguyen Van A Partners', '0307654321', 'Nguyen Van A', '0909123456', 'partner.a@gmail.com', 5.0)
    RETURNING id INTO f_partner;

    -- =================================================
    -- 2. SEED STORES (Cửa hàng)
    -- Lưu ý: Latitude/Longitude là toạ độ thật ở TP.HCM để test tính năng "Tìm quanh đây"
    -- =================================================

    -- Store 1: Quận 1 (Notre Dame Cathedral area) - Của Cty Mẹ
    INSERT INTO stores (code, name, address, phone, email, image_url, latitude, longitude, type, status, franchisee_id)
    VALUES ('STR-Q1-001', 'Coffee Central Q1', '1 Cong Xa Paris, Dist 1, HCM', '02811112222', 'store.q1@hq.com', 'https://img.com/store1.jpg', 10.779785, 106.699018, 0, 1, f_hq)
    RETURNING id INTO s_q1;

    -- Store 2: Quận 3 (Turtle Lake area) - Của Cty Mẹ
    INSERT INTO stores (code, name, address, phone, email, image_url, latitude, longitude, type, status, franchisee_id)
    VALUES ('STR-Q3-002', 'Coffee Lake Q3', '45 Pham Ngoc Thach, Dist 3, HCM', '02833334444', 'store.q3@hq.com', 'https://img.com/store2.jpg', 10.782877, 106.695847, 0, 1, f_hq)
    RETURNING id INTO s_q3;

    -- Store 3: Thủ Đức (Gigamall area) - Nhượng quyền
    INSERT INTO stores (code, name, address, phone, email, image_url, latitude, longitude, type, status, franchisee_id)
    VALUES ('STR-TD-003', 'Coffee Giga TD', '242 Pham Van Dong, Thu Duc, HCM', '02855556666', 'store.td@partner.com', 'https://img.com/store3.jpg', 10.827641, 106.721458, 1, 1, f_partner)
    RETURNING id INTO s_td;

    -- =================================================
    -- 3. SEED STORE HOURS (Giờ hoạt động)
    -- Cấu hình: Mở cửa 07:00 - 22:00 tất cả các ngày
    -- =================================================
    FOR i IN 0..6 LOOP
        -- Store Q1
        INSERT INTO store_hours (store_id, day_of_week, open_time, close_time) VALUES (s_q1, i, '07:00', '22:00');
        -- Store Q3
        INSERT INTO store_hours (store_id, day_of_week, open_time, close_time) VALUES (s_q3, i, '07:00', '22:00');
        -- Store TD
        INSERT INTO store_hours (store_id, day_of_week, open_time, close_time) VALUES (s_td, i, '07:30', '22:30'); -- Mở trễ hơn xíu
    END LOOP;

    -- =================================================
    -- 4. SEED SHIFT TEMPLATES (Ca mẫu)
    -- =================================================
    
    -- Ca Sáng (chung toàn hệ thống - store_id NULL)
    INSERT INTO shift_templates (code, name, start_time, end_time, break_duration_minutes)
    VALUES ('MORNING', 'Ca Sáng Tiêu Chuẩn', '07:00', '15:00', 30)
    RETURNING id INTO tpl_morning;

    -- Ca Chiều
    INSERT INTO shift_templates (code, name, start_time, end_time, break_duration_minutes)
    VALUES ('EVENING', 'Ca Chiều Tiêu Chuẩn', '14:00', '22:00', 30)
    RETURNING id INTO tpl_evening;

    -- =================================================
    -- 5. SEED SCHEDULES (Lịch làm việc mẫu cho hôm nay)
    -- Giả lập: Mỗi quán có 1 nhân viên làm ca sáng hôm nay
    -- Lưu ý: user_id ở đây là UUID ngẫu nhiên vì DB này không thấy bảng users bên Identity.
    -- Khi chạy thật, bạn phải lấy UUID user thật từ Identity DB bỏ vào.
    -- =================================================

    -- Nhân viên tại Q1 (Ca Sáng hôm nay)
    INSERT INTO schedules (user_id, store_id, shift_template_id, shift_date, start_time, end_time, status, hourly_wage)
    VALUES (
        uuid_generate_v4(), -- Fake User ID
        s_q1, 
        tpl_morning, 
        cur_date, 
        (cur_date || ' 07:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        (cur_date || ' 15:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        1, -- Published
        25000
    );

    -- Nhân viên tại Q3 (Ca Chiều hôm nay)
    INSERT INTO schedules (user_id, store_id, shift_template_id, shift_date, start_time, end_time, status, hourly_wage)
    VALUES (
        uuid_generate_v4(), -- Fake User ID
        s_q3, 
        tpl_evening, 
        cur_date, 
        (cur_date || ' 14:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        (cur_date || ' 22:00:00')::timestamp AT TIME ZONE 'Asia/Ho_Chi_Minh',
        1, -- Published
        25000
    );

    -- In ra ID của các Store để bạn copy về update cho Identity DB nếu cần
    RAISE NOTICE 'Seed Completed!';
    RAISE NOTICE 'Store Q1 ID: %', s_q1;
    RAISE NOTICE 'Store Q3 ID: %', s_q3;
    RAISE NOTICE 'Store TD ID: %', s_td;

END $$;