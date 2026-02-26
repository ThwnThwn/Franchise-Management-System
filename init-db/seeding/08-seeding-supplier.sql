-- Bật extension dblink để lấy dữ liệu seed từ inventory_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS dblink;

-- SEED NHÀ CUNG CẤP & ĐƠN NHẬP HÀNG
DO $$
DECLARE
    sup_vinamilk UUID := uuid_generate_v4();
    sup_trungnguyen UUID := uuid_generate_v4();
    sup_nongsan UUID := uuid_generate_v4();
    po_1 UUID := uuid_generate_v4();
    po_2 UUID := uuid_generate_v4();
    rec RECORD;
BEGIN
    -- 1. Insert Suppliers
    INSERT INTO suppliers (id, code, name, contact_person, phone, email, address, tax_code) VALUES
    (sup_vinamilk, 'SUP-VNM', 'Công ty Cổ phần Sữa Việt Nam (Vinamilk)', 'Nguyễn Văn A', '0901234567', 'contact@vinamilk.com', 'TP.HCM', '0300588569'),
    (sup_trungnguyen, 'SUP-TNL', 'Tập đoàn Trung Nguyên Legend', 'Trần Thị B', '0909876543', 'sales@trungnguyen.com', 'Buôn Ma Thuột', '0304386868'),
    (sup_nongsan, 'SUP-NVS', 'Nông sản Việt Sạch', 'Lê Văn C', '0912345678', 'info@nongsanviet.com', 'Đà Lạt', '0312345678');

    -- 2. Đọc dữ liệu Ingredients thực tế từ Inventory_db thông qua dblink
    -- Lưu ý: host là 'localhost' tuỳ thuộc vào network của DB trong Docker/Local. 
    -- Do script chạy trong cùng 1 instance Postgres nên dùng user mặc định.
    FOR rec IN 
        SELECT id, name, base_unit 
        FROM dblink('dbname=inventory_db user=admin password=admin', 'SELECT id, name, base_unit FROM ingredients')
        AS t(id UUID, name VARCHAR, base_unit VARCHAR)
    LOOP
        -- Vinamilk supplies Sữa, Bột béo, Kem
        IF rec.name LIKE '%Sữa%' OR rec.name LIKE '%Bột Béo%' OR rec.name LIKE '%Kem%' THEN
            INSERT INTO supplier_ingredients (supplier_id, ingredient_id, ingredient_name, ingredient_unit, unit_price, lead_time_days, is_primary)
            VALUES (sup_vinamilk, rec.id, rec.name, rec.base_unit, 25000 + (random() * 10000), 2, TRUE);
        
        -- Trung Nguyen supplies Trà, Cà phê
        ELSIF rec.name LIKE '%Trà%' OR rec.name LIKE '%Cà Phê%' OR rec.name LIKE '%Matcha%' THEN
            INSERT INTO supplier_ingredients (supplier_id, ingredient_id, ingredient_name, ingredient_unit, unit_price, lead_time_days, is_primary)
            VALUES (sup_trungnguyen, rec.id, rec.name, rec.base_unit, 150000 + (random() * 50000), 3, TRUE);
            
        -- Nong san supplies Topping, Đường, Siro, Chanh, Mật Ong, Khác
        ELSE
            INSERT INTO supplier_ingredients (supplier_id, ingredient_id, ingredient_name, ingredient_unit, unit_price, lead_time_days, is_primary)
            VALUES (sup_nongsan, rec.id, rec.name, rec.base_unit, 50000 + (random() * 20000), 1, TRUE);
        END IF;
    END LOOP;

    -- 3. Insert Purchase Orders (Tạo đơn hàng Completed - Tức là Expense)
    INSERT INTO purchase_orders (id, code, supplier_id, status, payment_status, total_amount, net_amount, paid_amount, order_date, completed_at, notes) VALUES
    (po_1, 'PO-202602-001', sup_vinamilk, 2, 2, 0, 0, 0, CURRENT_TIMESTAMP - INTERVAL '5 days', CURRENT_TIMESTAMP - INTERVAL '3 days', 'Nhập sữa tươi tháng 2'),
    (po_2, 'PO-202602-002', sup_trungnguyen, 2, 2, 0, 0, 0, CURRENT_TIMESTAMP - INTERVAL '10 days', CURRENT_TIMESTAMP - INTERVAL '8 days', 'Nhập trà và cà phê đợt 1');

    -- 4. Insert Purchase Order Items (Gắn ngẫu nhiên với nguyên liệu của NCC)
    INSERT INTO purchase_order_items (purchase_order_id, ingredient_id, quantity, unit_price, total_price)
    SELECT po_1, ingredient_id, 10000, unit_price, 10000 * unit_price
    FROM supplier_ingredients
    WHERE supplier_id = sup_vinamilk LIMIT 3;

    INSERT INTO purchase_order_items (purchase_order_id, ingredient_id, quantity, unit_price, total_price)
    SELECT po_2, ingredient_id, 2000, unit_price, 2000 * unit_price
    FROM supplier_ingredients
    WHERE supplier_id = sup_trungnguyen LIMIT 3;
    
    -- 5. Calculate correct totals for Purchase Orders based on Items
    UPDATE purchase_orders
    SET total_amount = (SELECT COALESCE(SUM(total_price), 0) FROM purchase_order_items WHERE purchase_order_id = po_1),
        net_amount = (SELECT COALESCE(SUM(total_price), 0) FROM purchase_order_items WHERE purchase_order_id = po_1),
        paid_amount = (SELECT COALESCE(SUM(total_price), 0) FROM purchase_order_items WHERE purchase_order_id = po_1)
    WHERE id = po_1;
    
    UPDATE purchase_orders
    SET total_amount = (SELECT COALESCE(SUM(total_price), 0) FROM purchase_order_items WHERE purchase_order_id = po_2),
        net_amount = (SELECT COALESCE(SUM(total_price), 0) FROM purchase_order_items WHERE purchase_order_id = po_2),
        paid_amount = (SELECT COALESCE(SUM(total_price), 0) FROM purchase_order_items WHERE purchase_order_id = po_2)
    WHERE id = po_2;

END $$;
