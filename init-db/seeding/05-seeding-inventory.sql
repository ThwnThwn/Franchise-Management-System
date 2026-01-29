-- Đảm bảo extension tồn tại
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. XÓA DỮ LIỆU CŨ (Nếu muốn làm sạch trước khi seed, hãy bỏ comment)
-- TRUNCATE TABLE "store_ingredients", "recipes", "ingredients" CASCADE;

-- 2. SEED NGUYÊN LIỆU (21 loại)
INSERT INTO "ingredients" ("name", "base_unit", "category") VALUES
('Trà Đen', 'ml', 'Liquid'),
('Trà Nhài', 'ml', 'Liquid'),
('Sữa Tươi', 'ml', 'Liquid'),
('Bột Béo', 'g', 'Solid'),
('Đường Nước', 'ml', 'Liquid'),
('Siro Đào', 'ml', 'Liquid'),
('Siro Dâu', 'ml', 'Liquid'),
('Hạt Trân Châu Đen', 'g', 'Topping'),
('Thạch Trái Cây', 'g', 'Topping'),
('Kem Cheese', 'ml', 'Liquid'),
('Bột Cà Phê', 'g', 'Solid'),
('Hạt Sen', 'g', 'Topping'),
('Sữa Đặc', 'ml', 'Liquid'),
('Trà Ô Long', 'ml', 'Liquid'),
('Siro Bạc Hà', 'ml', 'Liquid'),
('Bột Matcha', 'g', 'Solid'),
('Đá Viên', 'g', 'Solid'),
('Vải Ngâm', 'g', 'Topping'),
('Đào Miếng', 'g', 'Topping'),
('Nước Cốt Chanh', 'ml', 'Liquid'),
('Mật Ong', 'ml', 'Liquid');

-- 3. SEED CÔNG THỨC (Hơn 10 công thức)
DO $$
DECLARE
    v_trasua_id UUID := uuid_generate_v4();
    v_tradao_id UUID := uuid_generate_v4();
    v_cafesua_id UUID := uuid_generate_v4();
    v_matcha_id UUID := uuid_generate_v4();
    v_olong_id UUID := uuid_generate_v4();
    v_tradau_id UUID := uuid_generate_v4();
    v_trachanh_id UUID := uuid_generate_v4();
    v_bacxiu_id UUID := uuid_generate_v4();
    v_trasen_id UUID := uuid_generate_v4();
    v_travai_id UUID := uuid_generate_v4();
    v_kemcheese_id UUID := uuid_generate_v4();
BEGIN
    -- Công thức 1: Trà Sữa (Trà đen + Sữa tươi + Đường)
    INSERT INTO "recipes" ("variant_id", "ingredient_id", "quantity") VALUES
    (v_trasua_id, (SELECT id FROM ingredients WHERE name = 'Trà Đen'), 150),
    (v_trasua_id, (SELECT id FROM ingredients WHERE name = 'Sữa Tươi'), 50),
    (v_trasua_id, (SELECT id FROM ingredients WHERE name = 'Đường Nước'), 20);

    -- Công thức 2: Trà Đào (Trà nhài + Siro đào + Đào miếng)
    INSERT INTO "recipes" ("variant_id", "ingredient_id", "quantity") VALUES
    (v_tradao_id, (SELECT id FROM ingredients WHERE name = 'Trà Nhài'), 200),
    (v_tradao_id, (SELECT id FROM ingredients WHERE name = 'Siro Đào'), 20),
    (v_tradao_id, (SELECT id FROM ingredients WHERE name = 'Đào Miếng'), 30);

    -- Công thức 3: Cà phê sữa
    INSERT INTO "recipes" ("variant_id", "ingredient_id", "quantity") VALUES
    (v_cafesua_id, (SELECT id FROM ingredients WHERE name = 'Bột Cà Phê'), 25),
    (v_cafesua_id, (SELECT id FROM ingredients WHERE name = 'Sữa Đặc'), 40);

    -- Công thức 4: Matcha Latte
    INSERT INTO "recipes" ("variant_id", "ingredient_id", "quantity") VALUES
    (v_matcha_id, (SELECT id FROM ingredients WHERE name = 'Bột Matcha'), 10),
    (v_matcha_id, (SELECT id FROM ingredients WHERE name = 'Sữa Tươi'), 150);

    -- Các công thức đơn lẻ khác để đủ số lượng > 10
    INSERT INTO "recipes" ("variant_id", "ingredient_id", "quantity") VALUES
    (v_olong_id, (SELECT id FROM ingredients WHERE name = 'Trà Ô Long'), 200),
    (v_tradau_id, (SELECT id FROM ingredients WHERE name = 'Siro Dâu'), 30),
    (v_trachanh_id, (SELECT id FROM ingredients WHERE name = 'Nước Cốt Chanh'), 20),
    (v_bacxiu_id, (SELECT id FROM ingredients WHERE name = 'Sữa Đặc'), 50),
    (v_trasen_id, (SELECT id FROM ingredients WHERE name = 'Hạt Sen'), 40),
    (v_travai_id, (SELECT id FROM ingredients WHERE name = 'Vải Ngâm'), 30),
    (v_kemcheese_id, (SELECT id FROM ingredients WHERE name = 'Kem Cheese'), 60);
END $$;

-- 4. SEED KHO HÀNG (4 kho với tình trạng khác nhau)
DO $$
DECLARE
    s_q1 UUID := uuid_generate_v4(); -- Quận 1
    s_q7 UUID := uuid_generate_v4(); -- Quận 7
    s_td UUID := uuid_generate_v4(); -- Thủ Đức
    s_on UUID := uuid_generate_v4(); -- Kho Online
    rec RECORD;
BEGIN
    -- Kho 1: Quận 1 - Có đầy đủ tất cả nguyên liệu (Test trường hợp OK)
    FOR rec IN SELECT id FROM ingredients LOOP
        INSERT INTO "store_ingredients" ("store_id", "ingredient_id", "quantity", "min_threshold")
        VALUES (s_q1, rec.id, 10000, 500);
    END LOOP;

    -- Kho 2: Quận 7 - Không có "Sữa Đặc" và "Bột Cà Phê" (Test trường hợp thiếu món)
    FOR rec IN SELECT id FROM ingredients WHERE name NOT IN ('Sữa Đặc', 'Bột Cà Phê') LOOP
        INSERT INTO "store_ingredients" ("store_id", "ingredient_id", "quantity", "min_threshold")
        VALUES (s_q7, rec.id, 5000, 300);
    END LOOP;

    -- Kho 3: Thủ Đức - Chỉ có Topping (Test trường hợp thiếu nguyên liệu cốt lõi)
    FOR rec IN SELECT id FROM ingredients WHERE category = 'Topping' LOOP
        INSERT INTO "store_ingredients" ("store_id", "ingredient_id", "quantity", "min_threshold")
        VALUES (s_td, rec.id, 2000, 100);
    END LOOP;

    -- Kho 4: Kho Online - Nguyên liệu tồn kho rất thấp (Test trường hợp dưới min_threshold)
    FOR rec IN SELECT id FROM ingredients LIMIT 5 LOOP
        INSERT INTO "store_ingredients" ("store_id", "ingredient_id", "quantity", "min_threshold")
        VALUES (s_on, rec.id, 50, 100);
    END LOOP;
END $$;