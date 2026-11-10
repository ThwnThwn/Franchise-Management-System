-- ================================================================
-- SEED DATA: NHÂN VIÊN TEST CHO THUẬT TOÁN GREEDY
-- Chạy trên: identity_db
-- Chạy SAU: realease-init-identity.sql
--
-- Tạo 6 NV (home_store = Store Q1) với wage khác nhau
-- Dùng chung cho: 11-seeding-greedy-test-franchise.sql
-- ================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Cleanup NV test cũ
DELETE FROM users WHERE email LIKE '%greedy-test%';

DO $$
DECLARE
    s_q1      UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd';
    r_staff   UUID := 'e2000000-0000-0000-0000-000000000003';
    pass_hash VARCHAR := '$2a$12$mbTUUQ8yfeZt9G736FQdbeBTfVY6CesLBxhLCuEYkkb6C96f4Hdj2';
BEGIN
    INSERT INTO users (id, first_name, last_name, email, username, password, role_id, date_of_birth, home_store_id, current_hourly_wage)
    VALUES
    ('aa000001-0000-0000-0000-000000000001', 'An',   'Nguyễn', 'an.greedy-test@cahongban.com',   'greedy_an',   pass_hash, r_staff, '2000-01-01', s_q1, 25000),
    ('aa000002-0000-0000-0000-000000000002', 'Bình', 'Trần',   'binh.greedy-test@cahongban.com', 'greedy_binh', pass_hash, r_staff, '2000-02-02', s_q1, 30000),
    ('aa000003-0000-0000-0000-000000000003', 'Chi',  'Lê',     'chi.greedy-test@cahongban.com',  'greedy_chi',  pass_hash, r_staff, '2000-03-03', s_q1, 30000),
    ('aa000004-0000-0000-0000-000000000004', 'Dũng', 'Phạm',   'dung.greedy-test@cahongban.com', 'greedy_dung', pass_hash, r_staff, '2000-04-04', s_q1, 35000),
    ('aa000005-0000-0000-0000-000000000005', 'Em',   'Hoàng',  'em.greedy-test@cahongban.com',   'greedy_em',   pass_hash, r_staff, '2000-05-05', s_q1, 40000),
    ('aa000006-0000-0000-0000-000000000006', 'Phúc', 'Võ',     'phuc.greedy-test@cahongban.com', 'greedy_phuc', pass_hash, r_staff, '2000-06-06', s_q1, 45000)
    ON CONFLICT DO NOTHING;

    RAISE NOTICE '✅ 6 NV Greedy test: An(25k), Bình(30k), Chi(30k), Dũng(35k), Em(40k), Phúc(45k)';
END $$;
