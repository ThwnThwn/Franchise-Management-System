-- ================================================================
-- SEED DATA: TEST AUTO-FILL (Phase 4)
-- Tao du lieu test cho thuat toan tu dong xep ca
-- Chay file nay SAU KHI da chay 01-init-seeding-identity.sql
-- va 06-seeding-franchise.sql
-- ================================================================

-- Don dep du lieu test cu (neu co)
DELETE FROM shift_applications WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%test auto-fill%'
);
DELETE FROM shift_assignments WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%test auto-fill%'
);
DELETE FROM shifts WHERE note LIKE '%test auto-fill%';
DELETE FROM staff_availabilities WHERE note LIKE '%test auto-fill%';

DO $$
DECLARE
    -- Store Q1 (da co san tu franchise seeding)
    s_q1 UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd';

    -- Staff Q1 (da co san tu identity seeding)
    -- staff_q1 co home_store_id = s_q1
    u_staff_q1 UUID := '99999999-9999-9999-9999-999999999999';

    -- Manager Q1 (de lam reviewed_by neu can)
    -- u_manager1 UUID - lay tu DB

    -- ID ca test (co dinh de truy van de)
    shift_1 UUID := 'af000001-0000-0000-0000-000000000001'; -- Case 1: Ca Sang 10/3
    shift_2 UUID := 'af000002-0000-0000-0000-000000000002'; -- Case 1: Ca Chieu 10/3
    shift_3 UUID := 'af000003-0000-0000-0000-000000000003'; -- Case 1: Ca Sang 11/3 (can 2 nguoi)
    shift_6 UUID := 'af000006-0000-0000-0000-000000000006'; -- Case 6: NV bao ban
    shift_7a UUID := 'af000007-0000-0000-0000-0000000000a7'; -- Case 7: Ca Published (trung gio)
    shift_7b UUID := 'af000007-0000-0000-0000-0000000000b7'; -- Case 7: Ca Draft (can autofill)
    shift_8a UUID := 'af000008-0000-0000-0000-0000000000a8'; -- Case 8: Ca Toi (Published)
    shift_8b UUID := 'af000008-0000-0000-0000-0000000000b8'; -- Case 8: Ca Sang (Draft, nghi chua du)
    shift_9 UUID := 'af000009-0000-0000-0000-000000000009'; -- Case 9: Ca da co 1/2 nguoi

    -- ID lich ban (co dinh)
    avail_6 UUID := 'af100006-0000-0000-0000-000000000006';

BEGIN

    -- ================================================================
    -- CASE 1-5: CA DRAFT CO BAN (AutoFill binh thuong + idempotent)
    -- 3 ca Draft, tong 4 slot (1 + 1 + 2)
    -- ================================================================

    -- Ca Sang ngay 10/3: 07:00 - 15:00, can 1 nguoi
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_1, s_q1, '2026-03-10', '07:00', '15:00', 1, 0, 'Ca sang test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- Ca Chieu ngay 10/3: 15:00 - 23:00, can 1 nguoi
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_2, s_q1, '2026-03-10', '15:00', '23:00', 1, 0, 'Ca chieu test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- Ca Sang ngay 11/3: 07:00 - 15:00, can 2 nguoi
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_3, s_q1, '2026-03-11', '07:00', '15:00', 2, 0, 'Ca sang can 2 nguoi test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 6: NHAN VIEN BAO BAN TRUNG GIO CA
    -- staff_q1 bao ban ngay 17/3 sang 07-15
    -- ================================================================

    -- Ca Draft ngay 17/3: 07:00 - 15:00
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_6, s_q1, '2026-03-17', '07:00', '15:00', 1, 0, 'Ca test ban test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- staff_q1 bao BAN ngay 17/3 sang 07-15 (IsAvailable = false)
    INSERT INTO staff_availabilities (id, user_id, is_recurring, specific_date, day_of_week, start_time, end_time, is_available, note, created_at, updated_at, is_deleted)
    VALUES (avail_6, u_staff_q1, false, '2026-03-17', null, '07:00', '15:00', false, 'Em ban di kham benh test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 7: TRUNG CA DA PUBLISHED
    -- staff_q1 da co ca Published ngay 18/3 sang 07-15
    -- AutoFill phai skip staff_q1 khi xep ca Draft cung gio
    -- ================================================================

    -- Ca Published (da co) ngay 18/3: 07:00 - 15:00, staff_q1 da duoc gan
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_7a, s_q1, '2026-03-18', '07:00', '15:00', 1, 1, 'Ca Published da co nguoi test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (uuid_generate_v4(), shift_7a, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- Ca Draft (chua co ai) ngay 18/3: 07:00 - 15:00, can AutoFill
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_7b, s_q1, '2026-03-18', '07:00', '15:00', 1, 0, 'Ca Draft trung gio test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 8: NGHI CHUA DU 8 GIO (REST TIME)
    -- staff_q1 co ca Toi Published: 18/3 22:00 - 19/3 06:00
    -- AutoFill phai skip staff_q1 cho ca Sang 19/3 07:00-15:00
    -- (chi nghi 1 gio, chua du 8 gio)
    -- ================================================================

    -- Ca Toi (Published) ngay 18/3: 22:00 - 06:00 (sang 19/3)
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_8a, s_q1, '2026-03-18', '22:00', '06:00', 1, 1, 'Ca toi Published test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (uuid_generate_v4(), shift_8a, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- Ca Sang (Draft) ngay 19/3: 07:00 - 15:00
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_8b, s_q1, '2026-03-19', '07:00', '15:00', 1, 0, 'Ca sang nghi chua du test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 9: CA DA CO 1/2 NGUOI (PARTIAL FILL)
    -- Ca can 2 nguoi nhung da co 1, AutoFill chi them 1
    -- ================================================================

    -- Ca ngay 20/3: 07:00 - 15:00, can 2 nguoi
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_9, s_q1, '2026-03-20', '07:00', '15:00', 2, 0, 'Ca da co 1 nguoi test auto-fill', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- staff_q1 da duoc gan truoc do
    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (uuid_generate_v4(), shift_9, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    RAISE NOTICE 'Auto-Fill test data seeded thanh cong!';
END $$;
