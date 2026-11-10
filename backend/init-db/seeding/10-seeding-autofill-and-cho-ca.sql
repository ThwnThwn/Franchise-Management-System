-- ================================================================
-- SEED: TEST AUTO-FILL + CA TRỐNG ĐỂ ĐẨY LÊN CHỢ CA
-- Chạy SAU: realease-init-identity.sql, realease-init-franchise.sql
--
-- Nội dung:
--   1. Ca Draft có một phần đã gán → test AutoFill điền nốt.
--   2. Ca Draft trống hoàn toàn (0 người) → đẩy lên Chợ Ca (BulkOpenShifts).
-- Dùng ngày tương đối (CURRENT_DATE + n) để chạy bất kỳ lúc nào.
-- ================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Dọn dữ liệu test cũ (theo note)
DELETE FROM shift_applications WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%[autofill-cho-ca]%'
);
DELETE FROM shift_assignments WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%[autofill-cho-ca]%'
);
DELETE FROM shifts WHERE note LIKE '%[autofill-cho-ca]%';

DO $$
DECLARE
    s_q1     UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd';
    u_staff1 UUID := '99999999-9999-9999-9999-999999999999';
    u_staff4 UUID := '66666666-6666-6666-6666-666666666666';

    -- Ngày dùng cho ca (tương lai để AutoFill/BulkOpen hợp lệ)
    d1 DATE := CURRENT_DATE + 1;
    d2 DATE := CURRENT_DATE + 2;
    d3 DATE := CURRENT_DATE + 3;

    -- ID ca cố định (dễ gọi API test / BulkOpenShifts)
    -- Nhóm AutoFill (một phần đã gán hoặc trống)
    id_af1   UUID := 'a0af0001-0000-0000-0000-000000000001';
    id_af2   UUID := 'a0af0002-0000-0000-0000-000000000002';
    id_af3   UUID := 'a0af0003-0000-0000-0000-000000000003';
    -- Nhóm Chợ Ca (trống, để đẩy lên Open)
    id_cc1   UUID := 'a0cc0001-0000-0000-0000-000000000001';
    id_cc2   UUID := 'a0cc0002-0000-0000-0000-000000000002';
    id_cc3   UUID := 'a0cc0003-0000-0000-0000-000000000003';
BEGIN
    -- Đảm bảo ShiftRule cho Q1 (AutoFill cần min_hours_between_shifts, max_hours_per_week)
    DELETE FROM shift_rules WHERE store_id = s_q1;
    INSERT INTO shift_rules (id, store_id, allow_shift_swap, min_hours_between_shifts, max_hours_per_week, grace_period_minutes, daily_ot_threshold_hours, weekly_ot_threshold_hours, created_at, updated_at)
    VALUES (uuid_generate_v4(), s_q1, true, 8, 48, 5, 8, 40, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    -- ================================================================
    -- NHÓM A: TEST AUTO-FILL
    -- Ca Draft, một số đã gán 1/2 hoặc 0/2 → chạy AutoFill điền nốt
    -- ================================================================

    -- A1: Ca sáng d1, cần 2, đã gán 1 (staff1) → AutoFill thêm 1
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note, lock_version, created_at, updated_at, is_deleted)
    VALUES (id_af1, s_q1, d1, '07:00', '15:00', 2, 30, 0, 'AutoFill: ca can 2 da co 1 [autofill-cho-ca]', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);
    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (uuid_generate_v4(), id_af1, u_staff1, 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- A2: Ca chiều d1, cần 1, chưa ai → AutoFill thêm 1
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note, lock_version, created_at, updated_at, is_deleted)
    VALUES (id_af2, s_q1, d1, '15:00', '23:00', 1, 30, 0, 'AutoFill: ca chieu trong [autofill-cho-ca]', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- A3: Ca sáng d2, cần 2, chưa ai → AutoFill thêm 2
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note, lock_version, created_at, updated_at, is_deleted)
    VALUES (id_af3, s_q1, d2, '07:00', '15:00', 2, 30, 0, 'AutoFill: ca sang can 2 trong [autofill-cho-ca]', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- NHÓM B: CA TRỐNG ĐỂ ĐẨY LÊN CHỢ CA
    -- Draft, 0 người → gọi BulkOpenShifts(StoreId, [id_cc1, id_cc2, id_cc3])
    -- để chuyển sang Open, nhân viên có thể đăng ký nhận ca
    -- ================================================================

    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, break_duration_minutes, status, note, lock_version, created_at, updated_at, is_deleted)
    VALUES
    (id_cc1, s_q1, d2, '07:00', '15:00', 1, 30, 0, 'Cho Ca: ca sang trong [autofill-cho-ca]', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false),
    (id_cc2, s_q1, d2, '14:00', '22:00', 2, 30, 0, 'Cho Ca: ca chieu can 2 trong [autofill-cho-ca]', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false),
    (id_cc3, s_q1, d3, '07:00', '15:00', 1, 30, 0, 'Cho Ca: ca sang trong [autofill-cho-ca]', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    RAISE NOTICE 'Autofill + Cho Ca seed OK. AutoFill IDs: %, %, %. Cho Ca IDs: %, %, %', id_af1, id_af2, id_af3, id_cc1, id_cc2, id_cc3;
END $$;
