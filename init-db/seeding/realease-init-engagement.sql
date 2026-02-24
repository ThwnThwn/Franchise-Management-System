-- 1. Hạng thành viên
INSERT INTO "membership_tiers" ("id", "name", "min_spend_required", "earning_rate") VALUES
(uuid_generate_v4(), 'Bronze', 0, 1.0),
(uuid_generate_v4(), 'Silver', 5000000, 1.2),
(uuid_generate_v4(), 'Gold', 20000000, 1.5),
(uuid_generate_v4(), 'Platinum', 50000000, 2.0)
ON CONFLICT DO NOTHING;

-- 2. Tài khoản Loyalty
DO $$
DECLARE
    bronze_id UUID := (SELECT id FROM membership_tiers WHERE name = 'Bronze' LIMIT 1);
    gold_id UUID := (SELECT id FROM membership_tiers WHERE name = 'Gold' LIMIT 1);
    sample_ids UUID[] := ARRAY[
        '00964234-2ce1-4bbd-b926-64bf68fef73b', '01549a30-76eb-4148-9cae-97eb31b853d4',
        '07204f59-80da-4c1c-8a8f-462300a003de', '0d03e6fd-ae82-4183-aacd-707930a134fd',
        '134ff6db-eb5a-4bde-a92c-8faf8ffb37bf', '1ded7707-40d6-4467-997a-abeaf70d9470',
        '1ea54879-27d0-4b26-a0bf-60cb9b4f768a', '01feadb5-f1fc-45e9-8360-6fbdeea929ae', -- Đã fix UUID
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

-- 3. Vouchers mẫu
INSERT INTO "vouchers" ("code", "category", "type", "discount_value", "min_order_value", "point_cost", "init_quantity", "current_quantity", "end_date") VALUES
('WELCOME2026', 'DISCOUNT_GIFT', 'FIXED_AMOUNT', 50000, 200000, 0, 1000, 950, '2026-12-31'),
('FREESHIP', 'SHIPPING', 'FIXED_AMOUNT', 30000, 0, 50, 500, 480, '2026-06-01')
ON CONFLICT (code) WHERE is_deleted IS FALSE DO NOTHING;