-- 1. Xóa dữ liệu cũ để tránh xung đột (Tùy chọn)
TRUNCATE TABLE "point_logs", "user_vouchers", "voucher_usages", "voucher_items", "voucher_queue", "loyalty_accounts", "membership_tiers", "vouchers" CASCADE;

-- 2. Hạng thành viên (Membership Tiers)
-- Logic: 
-- Bronze: 10k = 1đ, 1đ = 1k, giảm 0%
-- Silver: 9k = 1đ, 1đ = 1.1k, giảm 3%
-- Gold: 8k = 1đ, 1đ = 1.2k, giảm 5%
-- Platinum: 7k = 1đ, 1đ = 1.5k, giảm 10%
INSERT INTO "membership_tiers" ("id", "name", "min_spend_required", "amount_per_point", "value_per_point", "discount_rate") VALUES
('00000000-0000-0000-0000-000000000001', 'Bronze', 0, 10000.000, 1000.000, 0.0),
('00000000-0000-0000-0000-000000000002', 'Silver', 1000000.000, 9000.000, 1100.000, 0.03),
('00000000-0000-0000-0000-000000000003', 'Gold', 2000000.000, 8000.000, 1200.000, 0.05),
('00000000-0000-0000-0000-000000000004', 'Platinum', 3000000.000, 7000.000, 1500.000, 0.10)
ON CONFLICT (id) DO NOTHING;

-- Seed dữ liệu cho bảng point_rules
-- Đơn vị tính: Ngày (expiry_days) - Mặc định tối thiểu 1 tháng (30 ngày)
INSERT INTO "point_rules" ("id", "reason_code", "expiry_days") VALUES
-- Nhóm cộng điểm (EARN)
(uuid_generate_v4(), 'ORDER_COMPLETED', 365),    -- 1 năm
(uuid_generate_v4(), 'BIRTHDAY_GIFT', 30),       -- 1 tháng (Tối thiểu)
(uuid_generate_v4(), 'PROMOTION_BONUS', 60),     -- 2 tháng
(uuid_generate_v4(), 'REFERRAL_REWARD', 180),    -- 6 tháng

-- Nhóm trừ điểm (REDEEM/EXPIRATION)
(uuid_generate_v4(), 'POINT_EXPIRED', 30),
(uuid_generate_v4(), 'POINT_REDEEM', 30),

-- Nhóm hoàn/hủy/điều chỉnh (REFUND/ADJUST)
(uuid_generate_v4(), 'ORDER_CANCEL_REFUND', 365), 
(uuid_generate_v4(), 'ORDER_RETURN_DEDUCT', 30),
(uuid_generate_v4(), 'MANUAL_ADJUST', 30)

ON CONFLICT ("reason_code") DO UPDATE SET 
    "expiry_days" = EXCLUDED."expiry_days",
    "updated_at" = CURRENT_TIMESTAMP;
-- 3. Tài khoản Loyalty (Dựa trên danh sách UUID người dùng của bạn)
DO $$
DECLARE
    bronze_id UUID := '00000000-0000-0000-0000-000000000001';
    gold_id UUID := '00000000-0000-0000-0000-000000000003';
    sample_ids UUID[] := ARRAY[
        '00964234-2ce1-4bbd-b926-64bf68fef73b', '01549a30-76eb-4148-9cae-97eb31b853d4',
        '07204f59-80da-4c1c-8a8f-462300a003de', '0d03e6fd-ae82-4183-aacd-707930a134fd',
        '134ff6db-eb5a-4bde-a92c-8faf8ffb37bf', '1ded7707-40d6-4467-997a-abeaf70d9470',
        '1ea54879-27d0-4b26-a0bf-60cb9b4f768a', '01feadb5-f1fc-45e9-8360-6fbdeea929ae',
        '24a66acc-f2cd-4e58-9b47-f17e19df80aa', '26c7f49d-c1ef-4f65-bab3-71f12f42e51c',
        '2a9dd74f-3284-4a0b-8cfc-0db5dbd43664', '2b9b0400-f37e-411a-ab97-7c58876182d6',
        '2e4194b0-649a-4103-ab63-15e5cc5cb92b', '3014aef9-f746-443f-85d8-a700185278db',
        '3707976c-40cf-4525-82af-88d22d23f56f'
    ];
    t_id UUID;
BEGIN
    FOREACH t_id IN ARRAY sample_ids LOOP
        INSERT INTO "loyalty_accounts" ("id", "available_points", "total_spent", "current_tier_id")
        VALUES (
            t_id, 
            CASE WHEN t_id = '0d03e6fd-ae82-4183-aacd-707930a134fd' THEN 1200 ELSE 0 END,
            CASE WHEN t_id = '0d03e6fd-ae82-4183-aacd-707930a134fd' THEN 15000000 ELSE 0 END,
            CASE WHEN t_id = '0d03e6fd-ae82-4183-aacd-707930a134fd' THEN gold_id ELSE bronze_id END
        )
        ON CONFLICT (id) DO NOTHING;
    END LOOP;
END $$;

DO $$
DECLARE
    -- Cấu hình tham số
    target_user_id UUID := '00964234-2ce1-4bbd-b926-64bf68fef73b';
    new_total_points DECIMAL(15, 3) := 100000.000;
    
    -- Biến tạm để tính toán log
    old_balance DECIMAL(15, 3);
    point_diff DECIMAL(15, 3);
BEGIN
    -- 1. Kiểm tra và lấy số dư hiện tại
    SELECT available_points INTO old_balance 
    FROM "loyalty_accounts" 
    WHERE "id" = target_user_id;

    -- Nếu user chưa có dòng trong loyalty_accounts, tạo mới luôn (đề phòng)
    IF NOT FOUND THEN
        INSERT INTO "loyalty_accounts" ("id", "available_points", "total_spent", "current_tier_id")
        VALUES (target_user_id, new_total_points, 0, '00000000-0000-0000-0000-000000000001');
        old_balance := 0;
    ELSE
        -- Nếu đã có, tiến hành cập nhật
        UPDATE "loyalty_accounts"
        SET "available_points" = new_total_points,
            "updated_at" = CURRENT_TIMESTAMP
        WHERE "id" = target_user_id;
    END IF;

    -- Tính toán số điểm chênh lệch thực tế để ghi log amount
    point_diff := new_total_points - old_balance;

    -- 2. Ghi Log biến động số dư (Audit Trail)
    INSERT INTO "point_logs" (
        "id",
        "user_id",
        "amount",
        "balance_before",
        "balance_after",
        "action_type",
        "reason_code",
        "description",
        "expired_at",
        "created_at"
    )
    VALUES (
        uuid_generate_v4(),
        target_user_id,
        point_diff,          -- Số điểm thay đổi (dương là cộng, âm là trừ)
        old_balance,         -- Số dư cũ
        new_total_points,    -- Số dư mới (100,000)
        'ADJUST',            -- Enum point_action_type
        'MANUAL_ADJUST',     -- Enum point_reason_code
        'System manual set points to 100,000 for system testing',
        CURRENT_TIMESTAMP + INTERVAL '1 year', -- Hết hạn sau 1 năm
        CURRENT_TIMESTAMP
    );

    RAISE NOTICE 'User % balance updated: % -> %', target_user_id, old_balance, new_total_points;
END $$;