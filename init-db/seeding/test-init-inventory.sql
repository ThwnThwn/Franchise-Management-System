-- INVENTORY SERVICE SEED
TRUNCATE store_ingredients, recipes, ingredients CASCADE;

INSERT INTO ingredients (id, name, base_unit, category) VALUES
('10000000-0000-0000-0000-000000000001', 'Bột Cà Phê', 'g', 'Solid'),
('10000000-0000-0000-0000-000000000002', 'Sữa Đặc', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000003', 'Sữa Tươi', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000004', 'Đường Nước', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000005', 'Đá Viên', 'g', 'Solid'),
('10000000-0000-0000-0000-000000000006', 'Trà Đen', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000007', 'Siro Đào', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000008', 'Đào Miếng', 'g', 'Topping'),
('10000000-0000-0000-0000-000000000009', 'Siro Dâu', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000010', 'Bột Matcha', 'g', 'Solid'),
('10000000-0000-0000-0000-000000000011', 'Trà Ô Long', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000012', 'Hạt Trân Châu', 'g', 'Topping'),
('10000000-0000-0000-0000-000000000013', 'Thạch Trái Cây', 'g', 'Topping'),
('10000000-0000-0000-0000-000000000014', 'Kem Cheese', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000015', 'Nước Cốt Chanh', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000016', 'Mật Ong', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000017', 'Bột Béo', 'g', 'Solid'),
('10000000-0000-0000-0000-000000000018', 'Trà Nhài', 'ml', 'Liquid'),
('10000000-0000-0000-0000-000000000019', 'Hạt Sen', 'g', 'Topping'),
('10000000-0000-0000-0000-000000000020', 'Siro Bạc Hà', 'ml', 'Liquid');

DO $$
DECLARE
    s_ids UUID[] := ARRAY['b7ac8328-2ca3-470d-80ca-15444eeab2dd', 'e1488c32-1e33-498f-9130-9e13f841c8ea', 'da4aa30d-be1e-4e1e-874a-feaec06c6169'];
    v_id UUID;
    i INT;
    ing RECORD;
BEGIN
    FOR i IN 1..3 LOOP
        FOR ing IN SELECT id FROM ingredients LOOP
            INSERT INTO store_ingredients (store_id, ingredient_id, quantity, min_threshold)
            VALUES (s_ids[i], ing.id, 100000, 1000);
        END LOOP;
    END LOOP;

    FOR i IN 1..10 LOOP
        FOR size_idx IN 1..3 LOOP
            -- Đồng bộ prefix 'b' với Product Service
            v_id := (('b' || size_idx || '000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0')))::UUID;
            
            INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES 
            (v_id, '10000000-0000-0000-0000-000000000005', 50 * size_idx), -- Đá
            (v_id, '10000000-0000-0000-0000-000000000004', 10 * size_idx); -- Đường
        END LOOP;
    END LOOP;
END $$;