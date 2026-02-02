-- PRODUCT SERVICE SEED
TRUNCATE product_variants, products, categories CASCADE;

INSERT INTO categories (id, name) VALUES 
('c1000000-0000-0000-0000-000000000001', 'Cà Phê'),
('c1000000-0000-0000-0000-000000000002', 'Trà');

DO $$
DECLARE
    p_id UUID;
    cat_cafe UUID := 'c1000000-0000-0000-0000-000000000001';
    cat_tra UUID := 'c1000000-0000-0000-0000-000000000002';
    names TEXT[] := ARRAY['Cà Phê Đen', 'Cà Phê Sữa', 'Bạc Xỉu', 'Americano', 'Latte', 'Trà Đào', 'Trà Dâu', 'Trà Sữa', 'Matcha Latte', 'Trà Ô Long'];
    i INT;
    current_cat UUID;
BEGIN
    FOR i IN 1..10 LOOP
        p_id := ('a0000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0'))::UUID;
        current_cat := CASE WHEN i <= 5 THEN cat_cafe ELSE cat_tra END;

        INSERT INTO products (id, name, category_id) VALUES (p_id, names[i], current_cat);

        -- b đại diện cho variant (hợp lệ Hex), 1/2/3 đại diện cho S/M/L
        INSERT INTO product_variants (id, product_id, sku, name, attributes, base_price, cost)
        VALUES (('b1000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0'))::UUID, p_id, UPPER(names[i]) || '-S', 'Size S', '{"size":"S"}', 25000, 10000);
        
        INSERT INTO product_variants (id, product_id, sku, name, attributes, base_price, cost)
        VALUES (('b2000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0'))::UUID, p_id, UPPER(names[i]) || '-M', 'Size M', '{"size":"M"}', 35000, 15000);

        INSERT INTO product_variants (id, product_id, sku, name, attributes, base_price, cost)
        VALUES (('b3000000-0000-0000-0000-0000000000' || LPAD(i::text, 2, '0'))::UUID, p_id, UPPER(names[i]) || '-L', 'Size L', '{"size":"L"}', 45000, 20000);
    END LOOP;
END $$;