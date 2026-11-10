-- ============================================================
-- WAREHOUSE SERVICE SEED
-- ============================================================

TRUNCATE stock_transactions, warehouse_transfers, warehouse_items, warehouses CASCADE;

DO $$
DECLARE
    s_q1 UUID := 'b7ac8328-2ca3-470d-80ca-15444eeab2dd';
    s_q3 UUID := 'e1488c32-1e33-498f-9130-9e13f841c8ea';
    s_td UUID := 'da4aa30d-be1e-4e1e-874a-feaec06c6169';

    w_q1 UUID := uuid_generate_v4();
    w_q3 UUID := uuid_generate_v4();
    w_td UUID := uuid_generate_v4();

    sys_user UUID := 'f0000000-0000-0000-0000-000000000001';

    ingredient_ids  UUID[]  := ARRAY[
        '10000000-0000-0000-0000-000000000001'::UUID,
        '10000000-0000-0000-0000-000000000002'::UUID,
        '10000000-0000-0000-0000-000000000003'::UUID,
        '10000000-0000-0000-0000-000000000004'::UUID,
        '10000000-0000-0000-0000-000000000005'::UUID,
        '10000000-0000-0000-0000-000000000006'::UUID,
        '10000000-0000-0000-0000-000000000007'::UUID,
        '10000000-0000-0000-0000-000000000008'::UUID,
        '10000000-0000-0000-0000-000000000009'::UUID,
        '10000000-0000-0000-0000-000000000010'::UUID,
        '10000000-0000-0000-0000-000000000011'::UUID,
        '10000000-0000-0000-0000-000000000012'::UUID,
        '10000000-0000-0000-0000-000000000013'::UUID,
        '10000000-0000-0000-0000-000000000014'::UUID,
        '10000000-0000-0000-0000-000000000015'::UUID,
        '10000000-0000-0000-0000-000000000016'::UUID,
        '10000000-0000-0000-0000-000000000017'::UUID,
        '10000000-0000-0000-0000-000000000018'::UUID,
        '10000000-0000-0000-0000-000000000019'::UUID,
        '10000000-0000-0000-0000-000000000020'::UUID
    ];

    -- base_unit từ inventory seed, index tương ứng 1-1 với ingredient_ids
    ingredient_units TEXT[] := ARRAY[
        'g',   -- Bột Cà Phê
        'ml',  -- Sữa Đặc
        'ml',  -- Sữa Tươi
        'ml',  -- Đường Nước
        'g',   -- Đá Viên
        'ml',  -- Trà Đen
        'ml',  -- Siro Đào
        'g',   -- Đào Miếng
        'ml',  -- Siro Dâu
        'g',   -- Bột Matcha
        'ml',  -- Trà Ô Long
        'g',   -- Hạt Trân Châu
        'g',   -- Thạch Trái Cây
        'ml',  -- Kem Cheese
        'ml',  -- Nước Cốt Chanh
        'ml',  -- Mật Ong
        'g',   -- Bột Béo
        'ml',  -- Trà Nhài
        'g',   -- Hạt Sen
        'ml'   -- Siro Bạc Hà
    ];

    ing_id   UUID;
    ing_unit TEXT;
    item_q1  UUID;
    item_q3  UUID;
    item_td  UUID;
    i        INT;

BEGIN

-- ============================================================
-- 1. SEED WAREHOUSES
-- ============================================================

INSERT INTO warehouses (id, code, name, address, store_id, status, is_deleted, created_at, updated_at)
VALUES
    (w_q1, 'WH-Q1', 'Warehouse Quận 1',  '1 Cong Xa Paris, Dist 1',       s_q1, 1, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (w_q3, 'WH-Q3', 'Warehouse Quận 3',  '45 Pham Ngoc Thach, Dist 3',    s_q3, 1, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (w_td, 'WH-TD', 'Warehouse Thủ Đức', '242 Pham Van Dong, Thu Duc',    s_td, 1, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);


-- ============================================================
-- 2. SEED WAREHOUSE ITEMS + STOCK TRANSACTIONS
-- ============================================================

FOR i IN 1..array_length(ingredient_ids, 1) LOOP

    ing_id   := ingredient_ids[i];
    ing_unit := ingredient_units[i];
    item_q1  := uuid_generate_v4();
    item_q3  := uuid_generate_v4();
    item_td  := uuid_generate_v4();

    -- ── Warehouse Q1 ──────────────────────────────────────
    INSERT INTO warehouse_items (id, warehouse_id, ingredient_id, unit, quantity, base_price, is_deleted, created_at, updated_at)
    VALUES (item_q1, w_q1, ing_id, ing_unit, 50000, 1000, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    INSERT INTO stock_transactions (id, transaction_code, warehouse_id, warehouse_item_id, type, quantity, unit_price, total_price, status, created_by, is_deleted, created_at, updated_at)
    VALUES (uuid_generate_v4(), 'STOCKIN-Q1-' || LPAD(i::text, 2, '0'), w_q1, item_q1, 1, 50000, 1000, 50000000, 2, sys_user, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    -- ── Warehouse Q3 ──────────────────────────────────────
    INSERT INTO warehouse_items (id, warehouse_id, ingredient_id, unit, quantity, base_price, is_deleted, created_at, updated_at)
    VALUES (item_q3, w_q3, ing_id, ing_unit, 40000, 900, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    INSERT INTO stock_transactions (id, transaction_code, warehouse_id, warehouse_item_id, type, quantity, unit_price, total_price, status, created_by, is_deleted, created_at, updated_at)
    VALUES (uuid_generate_v4(), 'STOCKIN-Q3-' || LPAD(i::text, 2, '0'), w_q3, item_q3, 1, 40000, 900, 36000000, 2, sys_user, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    -- ── Warehouse Thủ Đức ─────────────────────────────────
    INSERT INTO warehouse_items (id, warehouse_id, ingredient_id, unit, quantity, base_price, is_deleted, created_at, updated_at)
    VALUES (item_td, w_td, ing_id, ing_unit, 30000, 800, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

    INSERT INTO stock_transactions (id, transaction_code, warehouse_id, warehouse_item_id, type, quantity, unit_price, total_price, status, created_by, is_deleted, created_at, updated_at)
    VALUES (uuid_generate_v4(), 'STOCKIN-TD-' || LPAD(i::text, 2, '0'), w_td, item_td, 1, 30000, 800, 24000000, 2, sys_user, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

END LOOP;


-- ============================================================
-- 3. SEED WAREHOUSE TRANSFER (Q1 → Q3 mẫu)
-- ============================================================

INSERT INTO warehouse_transfers (id, transfer_code, from_warehouse_id, to_warehouse_id, status, reason, created_by, is_deleted, created_at, updated_at)
VALUES (uuid_generate_v4(), 'TRF-0001', w_q1, w_q3, 5, 'Initial stock transfer sample', sys_user, false, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);


RAISE NOTICE 'Warehouse DB Seeded Successfully';

END $$;