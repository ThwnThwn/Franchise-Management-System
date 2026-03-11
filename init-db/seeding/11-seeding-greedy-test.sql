-- ================================================================
-- SEED DATA: TEST TOÀN DIỆN THUẬT TOÁN GREEDY AUTO-FILL
-- Chạy trên: franchise_db  
-- Chạy SAU: 11-seeding-greedy-test-identity.sql (identity_db)
--
-- Store test: Coffee Central Q1 (b7ac8328-2ca3-470d-80ca-15444eeab2dd)
--
-- 10 test case:
--   TC01: Happy Path — ca trống, đủ NV
--   TC02: Ưu tiên công bằng — NV ít giờ trước
--   TC03: Cùng giờ → lương thấp trước (tiết kiệm)
--   TC04: NV báo BẬN → bị skip
--   TC05: Trùng ca Published (Overlap) → bị skip
--   TC06: Nghỉ chưa đủ 8h (Rest Time) → bị skip
--   TC07: Vượt giờ tuần 48h (Max Hours) → bị skip
--   TC08: Partial Fill (đã có 1/2 người)
--   TC09: Unfilled — không đủ NV hợp lệ
--   TC10: Idempotent — chạy 2 lần kết quả giống nhau
--
-- Dùng ngày tương đối (Thứ Hai tuần sau) để chạy bất kỳ lúc nào
-- ================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ================================================================
-- CLEANUP dữ liệu test cũ
-- ================================================================
DELETE FROM shift_applications WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%[greedy-test]%'
);
DELETE FROM shift_swap_requests WHERE source_assignment_id IN (
    SELECT sa.id FROM shift_assignments sa
    JOIN shifts s ON sa.shift_id = s.id WHERE s.note LIKE '%[greedy-test]%'
);
DELETE FROM time_logs WHERE shift_assignment_id IN (
    SELECT sa.id FROM shift_assignments sa
    JOIN shifts s ON sa.shift_id = s.id WHERE s.note LIKE '%[greedy-test]%'
);
DELETE FROM shift_assignments WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%[greedy-test]%'
);
DELETE FROM shifts WHERE note LIKE '%[greedy-test]%';
DELETE FROM staff_availabilities WHERE note LIKE '%[greedy-test]%';

DO $$
DECLARE
    -- ============================================================
    -- CONSTANTS
    -- ============================================================
    s_q1 UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd';

    -- 6 NV (khớp ID với file identity)
    nv_an    UUID := 'aa000001-0000-0000-0000-000000000001'; -- 25k/h
    nv_binh  UUID := 'aa000002-0000-0000-0000-000000000002'; -- 30k/h
    nv_chi   UUID := 'aa000003-0000-0000-0000-000000000003'; -- 30k/h
    nv_dung  UUID := 'aa000004-0000-0000-0000-000000000004'; -- 35k/h
    nv_em    UUID := 'aa000005-0000-0000-0000-000000000005'; -- 40k/h
    nv_phuc  UUID := 'aa000006-0000-0000-0000-000000000006'; -- 45k/h

    -- Ngày test (Thứ Hai tuần tới)
    next_monday DATE;
    d_mon DATE; d_tue DATE; d_wed DATE; d_thu DATE; d_fri DATE; d_sat DATE;

    -- ID ca test (cố định để verify)
    tc01_shift  UUID := 'cc010001-0000-0000-0000-000000000001';
    tc02_pre    UUID := 'cc020001-0000-0000-0000-000000000001';
    tc02_draft  UUID := 'cc020002-0000-0000-0000-000000000002';
    tc03_shift  UUID := 'cc030001-0000-0000-0000-000000000001';
    tc04_shift  UUID := 'cc040001-0000-0000-0000-000000000001';
    tc05_pub    UUID := 'cc050001-0000-0000-0000-000000000001';
    tc05_draft  UUID := 'cc050002-0000-0000-0000-000000000002';
    tc06_night  UUID := 'cc060001-0000-0000-0000-000000000001';
    tc06_draft  UUID := 'cc060002-0000-0000-0000-000000000002';
    tc07_p1     UUID := 'cc070001-0000-0000-0000-000000000001';
    tc07_p2     UUID := 'cc070002-0000-0000-0000-000000000002';
    tc07_p3     UUID := 'cc070003-0000-0000-0000-000000000003';
    tc07_p4     UUID := 'cc070004-0000-0000-0000-000000000004';
    tc07_p5     UUID := 'cc070005-0000-0000-0000-000000000005';
    tc07_draft  UUID := 'cc070006-0000-0000-0000-000000000006';
    tc08_shift  UUID := 'cc080001-0000-0000-0000-000000000001';
    tc09_pub1   UUID := 'cc090001-0000-0000-0000-000000000001';
    tc09_pub2   UUID := 'cc090002-0000-0000-0000-000000000002';
    tc09_pub3   UUID := 'cc090003-0000-0000-0000-000000000003';
    tc09_pub4   UUID := 'cc090004-0000-0000-0000-000000000004';
    tc09_draft  UUID := 'cc090005-0000-0000-0000-000000000005';

BEGIN
    -- Tính ngày Thứ Hai tuần tới
    next_monday := CURRENT_DATE + (8 - EXTRACT(ISODOW FROM CURRENT_DATE)::INT);
    IF EXTRACT(ISODOW FROM CURRENT_DATE) = 1 THEN
        next_monday := CURRENT_DATE + 7;
    END IF;
    d_mon := next_monday;
    d_tue := next_monday + 1;
    d_wed := next_monday + 2;
    d_thu := next_monday + 3;
    d_fri := next_monday + 4;
    d_sat := next_monday + 5;

    RAISE NOTICE '';
    RAISE NOTICE '══════════════════════════════════════════════════════════';
    RAISE NOTICE '  GREEDY AUTO-FILL TEST - Tuần: % → %', d_mon, d_sat;
    RAISE NOTICE '══════════════════════════════════════════════════════════';

    -- ============================================================
    -- SHIFT RULE cho Store Q1 (min_rest=8h, max_week=48h)
    -- ============================================================
    DELETE FROM shift_rules WHERE store_id = s_q1;
    INSERT INTO shift_rules (store_id, allow_shift_swap, min_hours_between_shifts, max_hours_per_week,
                             grace_period_minutes, daily_ot_threshold_hours, weekly_ot_threshold_hours)
    VALUES (s_q1, true, 8, 48, 5, 8, 40);

    -- ================================================================
    -- TC01: HAPPY PATH
    -- Ca Sáng Thứ Hai 07-15, cần 2, tất cả NV đều 0h
    -- Kỳ vọng: An(25k) + Bình(30k)
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc01_shift, s_q1, d_mon, '07:00', '15:00', 2, 30, 0,
            'TC01: Happy Path — ca sang can 2 nguoi [greedy-test]');

    -- ================================================================
    -- TC02: ƯU TIÊN CÔNG BẰNG
    -- An+Bình đã có Published Sáng Thứ Ba (7.5h)
    -- Ca Draft Chiều Thứ Ba cần 2 → chọn Chi(0h) + Dũng(0h)
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc02_pre, s_q1, d_tue, '07:00', '15:00', 2, 30, 1,
            'TC02: Published An+Binh da co 7.5h [greedy-test]');
    INSERT INTO shift_assignments (shift_id, user_id, status) VALUES
    (tc02_pre, nv_an, 0), (tc02_pre, nv_binh, 0);

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc02_draft, s_q1, d_tue, '15:00', '23:00', 2, 30, 0,
            'TC02: Ca chieu — uu tien NV it gio [greedy-test]');

    -- ================================================================
    -- TC03: CÙNG GIỜ → LƯƠNG THẤP TRƯỚC
    -- Ca Chiều Thứ Hai 15-23, cần 1
    -- Sau TC01 (An+Bình gán Virtual), NV còn: Chi(0h,30k), Dũng(0h,35k)...
    -- → Chọn Chi(30k)
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc03_shift, s_q1, d_mon, '15:00', '23:00', 1, 30, 0,
            'TC03: Cung gio → luong thap truoc [greedy-test]');

    -- ================================================================
    -- TC04: NV BÁO BẬN → SKIP
    -- An bận Thứ Tư 07-12, Ca Draft 07-15 → skip An
    -- ================================================================
    INSERT INTO staff_availabilities (user_id, is_recurring, specific_date, day_of_week, start_time, end_time, is_available, note)
    VALUES (nv_an, false, d_wed, null, '07:00', '12:00', false,
            'An ban di kham benh [greedy-test]');

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc04_shift, s_q1, d_wed, '07:00', '15:00', 1, 30, 0,
            'TC04: An bao ban → skip An [greedy-test]');

    -- ================================================================
    -- TC05: TRÙNG CA PUBLISHED (Overlap) → SKIP
    -- Bình Published Thứ Năm 08-16, Draft 07-15 trùng → skip Bình
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc05_pub, s_q1, d_thu, '08:00', '16:00', 1, 30, 1,
            'TC05: Binh Published 08-16 [greedy-test]');
    INSERT INTO shift_assignments (shift_id, user_id, status) VALUES (tc05_pub, nv_binh, 0);

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc05_draft, s_q1, d_thu, '07:00', '15:00', 1, 30, 0,
            'TC05: Draft trung gio Binh → skip [greedy-test]');

    -- ================================================================
    -- TC06: NGHỈ CHƯA ĐỦ 8H (Rest Time) → SKIP
    -- Dũng ca đêm Wed 22:00-06:00, Draft Thu 07-15 (nghỉ 1h < 8h)
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc06_night, s_q1, d_wed, '22:00', '06:00', 1, 0, 1,
            'TC06: Dung ca dem Wed 22-06 [greedy-test]');
    INSERT INTO shift_assignments (shift_id, user_id, status) VALUES (tc06_night, nv_dung, 0);

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc06_draft, s_q1, d_thu, '07:00', '15:00', 1, 30, 0,
            'TC06: Ca sang Thu — Dung nghi 1h < 8h → skip [greedy-test]');

    -- ================================================================
    -- TC07: VƯỢT GIỜ TUẦN 48H (Max Hours) → SKIP
    -- Phúc 5 ca Published Mon-Fri 07:30-17:30 (10h/ca, break=0) = 50h
    -- Ca Draft Sat 15-23 (7.5h) → 50+7.5=57.5h > 48h → skip
    -- Rest: Fri 17:30 → Sat 15:00 = 21.5h > 8h ✓
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES
    (tc07_p1, s_q1, d_mon, '07:30', '17:30', 1, 0, 1, 'TC07: Phuc Mon 7:30-17:30 [greedy-test]'),
    (tc07_p2, s_q1, d_tue, '07:30', '17:30', 1, 0, 1, 'TC07: Phuc Tue 7:30-17:30 [greedy-test]'),
    (tc07_p3, s_q1, d_wed, '07:30', '17:30', 1, 0, 1, 'TC07: Phuc Wed 7:30-17:30 [greedy-test]'),
    (tc07_p4, s_q1, d_thu, '07:30', '17:30', 1, 0, 1, 'TC07: Phuc Thu 7:30-17:30 [greedy-test]'),
    (tc07_p5, s_q1, d_fri, '07:30', '17:30', 1, 0, 1, 'TC07: Phuc Fri 7:30-17:30 [greedy-test]');

    INSERT INTO shift_assignments (shift_id, user_id, status) VALUES
    (tc07_p1, nv_phuc, 0), (tc07_p2, nv_phuc, 0), (tc07_p3, nv_phuc, 0),
    (tc07_p4, nv_phuc, 0), (tc07_p5, nv_phuc, 0);

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc07_draft, s_q1, d_sat, '15:00', '23:00', 1, 30, 0,
            'TC07: Ca Sat — Phuc 50h+7.5h>48h → skip [greedy-test]');

    -- ================================================================
    -- TC08: PARTIAL FILL (đã có 1/2 → chỉ thêm 1)
    -- Ca Fri 07-15, cần 2, đã gán An → thêm 1
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc08_shift, s_q1, d_fri, '07:00', '15:00', 2, 30, 0,
            'TC08: Partial Fill — da co An 1/2 [greedy-test]');
    INSERT INTO shift_assignments (shift_id, user_id, status) VALUES (tc08_shift, nv_an, 0);

    -- ================================================================
    -- TC09: UNFILLED — Cần 3 nhưng 0 NV hợp lệ
    -- Fri 15-23: Bình/Chi/Dũng/Em Published 14-22 (overlap)
    --            An rest<8h, Phúc max hours → 0/3 → UnfilledShiftIds
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES
    (tc09_pub1, s_q1, d_fri, '14:00', '22:00', 1, 0, 1, 'TC09: Binh Published Fri 14-22 [greedy-test]'),
    (tc09_pub2, s_q1, d_fri, '14:00', '22:00', 1, 0, 1, 'TC09: Chi Published Fri 14-22 [greedy-test]'),
    (tc09_pub3, s_q1, d_fri, '14:00', '22:00', 1, 0, 1, 'TC09: Dung Published Fri 14-22 [greedy-test]'),
    (tc09_pub4, s_q1, d_fri, '14:00', '22:00', 1, 0, 1, 'TC09: Em Published Fri 14-22 [greedy-test]');

    INSERT INTO shift_assignments (shift_id, user_id, status) VALUES
    (tc09_pub1, nv_binh, 0), (tc09_pub2, nv_chi, 0),
    (tc09_pub3, nv_dung, 0), (tc09_pub4, nv_em, 0);

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note)
    VALUES (tc09_draft, s_q1, d_fri, '15:00', '23:00', 3, 30, 0,
            'TC09: Can 3 nguoi nhung 0 NV hop le → unfilled [greedy-test]');

    -- ================================================================
    RAISE NOTICE '';
    RAISE NOTICE '══════════════════════════════════════════════════════════';
    RAISE NOTICE '  ✅ GREEDY TEST DATA SEEDED!';
    RAISE NOTICE '  Store Q1: %', s_q1;
    RAISE NOTICE '  Tuần: % → %', d_mon, d_sat;
    RAISE NOTICE '══════════════════════════════════════════════════════════';

END $$;
