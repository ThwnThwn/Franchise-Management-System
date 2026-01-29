-- SEED INGREDIENTS (Dùng prefix 'b')
INSERT INTO ingredients (id, name, base_unit) VALUES
('b0000001-0000-0000-0000-000000000000', 'Bột Cà Phê Robusta', 'g'),
('b0000002-0000-0000-0000-000000000000', 'Sữa Đặc', 'ml'),
('b0000003-0000-0000-0000-000000000000', 'Đường Nước', 'ml'),
('b0000004-0000-0000-0000-000000000000', 'Nước Sôi', 'ml'),
('b0000005-0000-0000-0000-000000000000', 'Đá Viên', 'g');

-- Công thức cho Cà Phê Sữa Size S (Dùng ID mới)
INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
('f0000008-0000-0000-0000-0000000000f1', 'b0000001-0000-0000-0000-000000000000', 20),
('f0000008-0000-0000-0000-0000000000f1', 'b0000002-0000-0000-0000-000000000000', 30),
('f0000008-0000-0000-0000-0000000000f1', 'b0000005-0000-0000-0000-000000000000', 150);

-- Công thức cho Cà Phê Sữa Size M
INSERT INTO recipes (variant_id, ingredient_id, quantity) VALUES
('f0000008-0000-0000-0000-0000000000f2', 'b0000001-0000-0000-0000-000000000000', 30),
('f0000008-0000-0000-0000-0000000000f2', 'b0000002-0000-0000-0000-000000000000', 45),
('f0000008-0000-0000-0000-0000000000f2', 'b0000005-0000-0000-0000-000000000000', 200);

-- INVENTORY SERVICE - STORE SEEDING
DO $$
DECLARE
    -- Danh sách 5 chi nhánh với UUID chuẩn
    store_ids UUID[] := ARRAY[
        'e0000000-0000-0000-0000-000000000001', -- Quận 1
        'e0000000-0000-0000-0000-000000000002', -- Quận 3
        'e0000000-0000-0000-0000-000000000003', -- Quận 7
        'e0000000-0000-0000-0000-000000000004', -- Bình Thạnh
        'e0000000-0000-0000-0000-000000000005'  -- Thủ Đức
    ];
    s_id UUID;
BEGIN
    FOREACH s_id IN ARRAY store_ids LOOP
        INSERT INTO "store_ingredients" ("store_id", "ingredient_id", "quantity", "min_threshold")
        SELECT 
            s_id, 
            id, 
            CASE 
                WHEN base_unit = 'g'  THEN 300  -- 15kg
                WHEN base_unit = 'ml' THEN 50  -- 25 lít
                ELSE 100 
            END,
            CASE 
                WHEN base_unit = 'g'  THEN 300 
                WHEN base_unit = 'ml' THEN 50 
                ELSE 50 
            END
        FROM "ingredients"
        ON CONFLICT (store_id, ingredient_id) DO UPDATE 
        SET quantity = EXCLUDED.quantity, 
            min_threshold = EXCLUDED.min_threshold;
    END LOOP;

    RAISE NOTICE 'Đã cập nhật tồn kho hợp lệ cho % chi nhánh.', array_length(store_ids, 1);
END $$;