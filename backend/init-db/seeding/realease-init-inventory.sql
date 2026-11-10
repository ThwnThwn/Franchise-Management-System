-- INVENTORY SERVICE SEED
TRUNCATE store_ingredients, recipes, ingredients, ingredient_categories CASCADE;

-- 1. Seed bảng danh mục trước để lấy ID
INSERT INTO ingredient_categories (id, name, description) VALUES
('c0000000-0000-0000-0000-000000000001', 'Solid', 'Nguyên liệu dạng rắn/bột'),
('c0000000-0000-0000-0000-000000000002', 'Liquid', 'Nguyên liệu dạng lỏng'),
('c0000000-0000-0000-0000-000000000003', 'Topping', 'Nguyên liệu trang trí/thêm');

-- 2. Seed bảng nguyên liệu với category_id tương ứng
INSERT INTO ingredients (id, name, base_unit, category_id) VALUES
('10000000-0000-0000-0000-000000000001', 'Bột Cà Phê', 'g', 'c0000000-0000-0000-0000-000000000001'),
('10000000-0000-0000-0000-000000000002', 'Sữa Đặc', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000003', 'Sữa Tươi', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000004', 'Đường Nước', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000005', 'Đá Viên', 'g', 'c0000000-0000-0000-0000-000000000001'),
('10000000-0000-0000-0000-000000000006', 'Trà Đen', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000007', 'Siro Đào', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000008', 'Đào Miếng', 'g', 'c0000000-0000-0000-0000-000000000003'),
('10000000-0000-0000-0000-000000000009', 'Siro Dâu', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000010', 'Bột Matcha', 'g', 'c0000000-0000-0000-0000-000000000001'),
('10000000-0000-0000-0000-000000000011', 'Trà Ô Long', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000012', 'Hạt Trân Châu', 'g', 'c0000000-0000-0000-0000-000000000003'),
('10000000-0000-0000-0000-000000000013', 'Thạch Trái Cây', 'g', 'c0000000-0000-0000-0000-000000000003'),
('10000000-0000-0000-0000-000000000014', 'Kem Cheese', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000015', 'Nước Cốt Chanh', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000016', 'Mật Ong', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000017', 'Bột Béo', 'g', 'c0000000-0000-0000-0000-000000000001'),
('10000000-0000-0000-0000-000000000018', 'Trà Nhài', 'ml', 'c0000000-0000-0000-0000-000000000002'),
('10000000-0000-0000-0000-000000000019', 'Hạt Sen', 'g', 'c0000000-0000-0000-0000-000000000003'),
('10000000-0000-0000-0000-000000000020', 'Siro Bạc Hà', 'ml', 'c0000000-0000-0000-0000-000000000002');

-- 3. Block xử lý logic cho Store Inventory và Recipes
DO $$
DECLARE
    s_ids UUID[] := ARRAY['b7ac8328-2ca3-470d-80ca-15444eeab2dd', 'e1488c32-1e33-498f-9130-9e13f841c8ea', 'da4aa30d-be1e-4e1e-874a-feaec06c6169'];
    v_id     UUID;
    v_id_hex TEXT;
    i        INT;
    ing      RECORD;
BEGIN
    -- Seed tồn kho cho từng cửa hàng
    FOR i IN 1..3 LOOP
        FOR ing IN SELECT id FROM ingredients LOOP
            INSERT INTO store_ingredients (store_id, ingredient_id, quantity, min_threshold)
            VALUES (s_ids[i], ing.id, 100000, 1000);
        END LOOP;
    END LOOP;

    -- Seed công thức (Recipe)
    FOR i IN 1..10 LOOP
        FOR size_idx IN 1..3 LOOP
            v_id_hex := 'b' || size_idx || '000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0');
            v_id := v_id_hex::UUID;

            -- Nguyên liệu chung: Đá + Đường
            INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                (v_id, '10000000-0000-0000-0000-000000000005', 50 * size_idx),  -- Đá Viên
                (v_id, '10000000-0000-0000-0000-000000000004', 10 * size_idx);  -- Đường Nước

            -- i=1: Cà Phê Đen
            IF i = 1 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000001', 15 * size_idx); -- Bột Cà Phê

            -- i=2: Cà Phê Sữa
            ELSIF i = 2 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000001', 15 * size_idx), -- Bột Cà Phê
                    (v_id, '10000000-0000-0000-0000-000000000002', 20 * size_idx); -- Sữa Đặc

            -- i=3: Bạc Xỉu
            ELSIF i = 3 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000001', 10 * size_idx), -- Bột Cà Phê
                    (v_id, '10000000-0000-0000-0000-000000000002', 25 * size_idx), -- Sữa Đặc
                    (v_id, '10000000-0000-0000-0000-000000000003', 30 * size_idx); -- Sữa Tươi

            -- i=4: Americano
            ELSIF i = 4 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000001', 20 * size_idx); -- Bột Cà Phê

            -- i=5: Latte
            ELSIF i = 5 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000001', 15 * size_idx), -- Bột Cà Phê
                    (v_id, '10000000-0000-0000-0000-000000000003', 50 * size_idx); -- Sữa Tươi

            -- i=6: Trà Đào
            ELSIF i = 6 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000006', 80 * size_idx), -- Trà Đen
                    (v_id, '10000000-0000-0000-0000-000000000007', 20 * size_idx), -- Siro Đào
                    (v_id, '10000000-0000-0000-0000-000000000008', 30 * size_idx); -- Đào Miếng

            -- i=7: Trà Dâu
            ELSIF i = 7 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000006', 80 * size_idx), -- Trà Đen
                    (v_id, '10000000-0000-0000-0000-000000000009', 20 * size_idx); -- Siro Dâu

            -- i=8: Trà Sữa
            ELSIF i = 8 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000006', 60 * size_idx), -- Trà Đen
                    (v_id, '10000000-0000-0000-0000-000000000002', 20 * size_idx), -- Sữa Đặc
                    (v_id, '10000000-0000-0000-0000-000000000017', 10 * size_idx); -- Bột Béo

            -- i=9: Matcha Latte
            ELSIF i = 9 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000010', 10 * size_idx), -- Bột Matcha
                    (v_id, '10000000-0000-0000-0000-000000000003', 60 * size_idx); -- Sữa Tươi

            -- i=10: Trà Ô Long
            ELSIF i = 10 THEN
                INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
                    (v_id, '10000000-0000-0000-0000-000000000011', 100 * size_idx); -- Trà Ô Long

            END IF;
        END LOOP;
    END LOOP;
END $$;