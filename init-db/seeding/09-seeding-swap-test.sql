-- ================================================================
-- SEED DATA: TEST SWAP AND RELEASE (Phase 5)
-- Tao du lieu test cho doi ca va nha ca
-- Chay file nay SAU KHI da chay:
-- 1. realease-init-identity.sql
-- 2. realease-init-franchise.sql
-- ================================================================

-- Don dep du lieu test cu
DELETE FROM shift_swap_requests WHERE reason LIKE '%test swap%';
DELETE FROM shift_assignments WHERE shift_id IN (
    SELECT id FROM shifts WHERE note LIKE '%test swap%'
);
DELETE FROM shifts WHERE note LIKE '%test swap%';

DO $$
DECLARE
    -- Store Q1 (da co san tu realease-init-franchise.sql)
    s_q1 UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd';

    -- Staff Q1 = A (nguoi gui yeu cau, da co san tu realease-init-franchise.sql)
    u_staff_q1 UUID := '99999999-9999-9999-9999-999999999999';

    -- Staff Q3 = B1 (nguoi nhan yeu cau KHAC Store, da co san)
    u_staff_q3 UUID := '88888888-8888-8888-8888-888888888888';

    -- Them 1 staff CUNG Store Q1 (de test swap hop le)
    -- id cung duoc lay tu realease-init-identity.sql (staff_test1)
    u_staff_q1_test1 UUID := '66666666-6666-6666-6666-666666666666';

    -- Manager Q1 (de test duyet)
    u_mana_q1 UUID := 'f0000000-0000-0000-0000-000000000001';

    -- Ca test (ID co dinh, ky tu HEX hop le)
    shift_swap_ok UUID      := 'f5510001-0000-0000-0000-000000000001';  -- Ca Published, tuong lai, de test swap
    shift_swap_draft UUID   := 'f5510002-0000-0000-0000-000000000002';  -- Ca Draft (khong cho swap)
    shift_swap_past UUID    := 'f5510003-0000-0000-0000-000000000003';  -- Ca Published, qua khu
    shift_release UUID      := 'f5510004-0000-0000-0000-000000000004';  -- Ca Published, de test release
    shift_release_2 UUID    := 'f5510005-0000-0000-0000-000000000005';  -- Ca Published, 2 nguoi (test Open)

    -- Assignment test (ID co dinh de dung trong body gRPC)
    asgn_swap UUID    := 'f55a0001-0000-0000-0000-000000000001';  -- Staff 1 duoc gan shift_swap_ok
    asgn_draft UUID   := 'f55a0002-0000-0000-0000-000000000002';  -- Staff 1 duoc gan shift_swap_draft
    asgn_past UUID    := 'f55a0003-0000-0000-0000-000000000003';  -- Staff 1 duoc gan shift_swap_past
    asgn_release UUID := 'f55a0004-0000-0000-0000-000000000004';  -- Staff 1 duoc gan shift_release
    asgn_rel2_a UUID  := 'f55a0005-0000-0000-0000-000000000005';  -- Staff 1 duoc gan shift_release_2
    asgn_rel2_b UUID  := 'f55a0006-0000-0000-0000-000000000006';  -- Staff 2 (test1) duoc gan shift_release_2

BEGIN

    -- ================================================================
    -- SHIFT RULE: Dam bao AllowShiftSwap = true cho Store Q1
    -- Xoa rule cu (neu co) roi insert moi
    -- ================================================================
    DELETE FROM shift_rules WHERE store_id = s_q1;
    INSERT INTO shift_rules (id, store_id, allow_shift_swap, created_at, updated_at)
    VALUES (uuid_generate_v4(), s_q1, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    -- ================================================================
    -- CASE 1: CA PUBLISHED TUONG LAI - SWAP HOP LE
    -- Staff 1 duoc gan ca sang ngay 15/3 (Published)
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_swap_ok, s_q1, '2026-03-15', '07:00', '15:00', 1, 1, 'Ca sang Published test swap', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (asgn_swap, shift_swap_ok, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 2: CA DRAFT - SWAP BI CHAN
    -- Ca Draft khong cho phep doi ca
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_swap_draft, s_q1, '2026-03-16', '07:00', '15:00', 1, 0, 'Ca Draft test swap', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (asgn_draft, shift_swap_draft, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 3: CA QUA KHU - SWAP BI CHAN
    -- Ca Published nhung da qua ngay
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_swap_past, s_q1, '2026-01-01', '07:00', '15:00', 1, 1, 'Ca qua khu test swap', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (asgn_past, shift_swap_past, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 4: CA DE TEST RELEASE (1 nguoi)
    -- Staff 1 nha ca -> ca chuyen thanh Open
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_release, s_q1, '2026-03-17', '07:00', '15:00', 1, 1, 'Ca Published test release test swap', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (asgn_release, shift_release, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    -- ================================================================
    -- CASE 5: CA 2 NGUOI - RELEASE 1 MA VAN CON DU
    -- Can 2, co 2 (Staff 1 + Staff 2). Staff 1 nha -> con 1 < 2 -> Open
    -- ================================================================
    INSERT INTO shifts (id, store_id, shift_date, start_time, end_time, required_staff_count, status, note, created_at, updated_at, is_deleted)
    VALUES (shift_release_2, s_q1, '2026-03-18', '07:00', '15:00', 2, 1, 'Ca 2 nguoi test release test swap', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (asgn_rel2_a, shift_release_2, u_staff_q1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    INSERT INTO shift_assignments (id, shift_id, user_id, status, created_at, updated_at, is_deleted)
    VALUES (asgn_rel2_b, shift_release_2, u_staff_q1_test1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false);

    RAISE NOTICE 'Swap and Release test data seeded thanh cong!';
END $$;
