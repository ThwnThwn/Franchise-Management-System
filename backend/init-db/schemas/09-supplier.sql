-- Bật extension cho UUID (NẾU CHƯA CÓ TRONG DB NÀY)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

--Table Suppliers

CREATE TABLE IF NOT EXISTS suppliers (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  contact_person VARCHAR(150),
  phone VARCHAR(20),
  email VARCHAR(150),
  address TEXT,
  tax_code VARCHAR(50),
  is_active BOOLEAN DEFAULT TRUE,
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX ux_suppliers_code
ON suppliers(code)
WHERE is_deleted = FALSE;
    
CREATE UNIQUE INDEX ux_suppliers_tax_code
ON suppliers(tax_code)
WHERE tax_code IS NOT NULL;

--Table Supplier Ingredients

CREATE TABLE IF NOT EXISTS supplier_ingredients (
  supplier_id UUID NOT NULL,
  ingredient_id UUID NOT NULL,
  ingredient_name VARCHAR(255) NOT NULL,
  ingredient_unit VARCHAR(50) NOT NULL,            -- Đơn vị (ml, g)
  unit_price DECIMAL(18,2) NOT NULL CHECK (unit_price >= 0),
  lead_time_days INT,
  is_primary BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (supplier_id, ingredient_id),

  FOREIGN KEY (supplier_id)
    REFERENCES suppliers(id)
    ON DELETE CASCADE
);

CREATE INDEX idx_supplier_ingredients_ingredient
ON supplier_ingredients(ingredient_id);

-- -----------------------------------------------------------------------------
-- Table Purchase Orders (Đơn nhập hàng - Nguồn sinh Chi phí Expense)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS purchase_orders (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  code VARCHAR(50) NOT NULL,
  supplier_id UUID NOT NULL,
  store_id UUID,                     -- Nhập về kho/cửa hàng nào
  
  status INT NOT NULL DEFAULT 0,     -- 0:Pending, 1:Processing, 2:Completed, 3:Cancelled
  payment_status INT NOT NULL DEFAULT 0, -- 0:Unpaid, 1:Partial, 2:Paid
  
  -- Tài chính (Dùng cho Báo cáo Chi phí)
  total_amount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (total_amount >= 0),
  discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (discount_amount >= 0),
  net_amount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (net_amount >= 0), -- EXPENSE AMOUNT
  paid_amount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (paid_amount >= 0),
  
  order_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  expected_delivery_date TIMESTAMP WITH TIME ZONE,
  completed_at TIMESTAMP WITH TIME ZONE,
  
  notes TEXT,
  reference_code VARCHAR(100),       -- Mã hóa đơn giấy từ nhà cung cấp
  
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (supplier_id)
    REFERENCES suppliers(id)
    ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ux_purchase_orders_code
ON purchase_orders(code)
WHERE is_deleted = FALSE;

CREATE INDEX idx_purchase_orders_supplier
ON purchase_orders(supplier_id);

CREATE INDEX idx_purchase_orders_status
ON purchase_orders(status);

-- -----------------------------------------------------------------------------
-- Table Purchase Order Items (Chi tiết đơn nhập)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS purchase_order_items (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  purchase_order_id UUID NOT NULL,
  ingredient_id UUID NOT NULL,       -- Relates to InventoryService
  
  quantity DECIMAL(12,3) NOT NULL CHECK (quantity > 0),    -- Số lượng theo đơn vị (g, ml)
  unit_price DECIMAL(18,2) NOT NULL CHECK (unit_price >= 0),
  total_price DECIMAL(18,2) NOT NULL CHECK (total_price >= 0),
  
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (purchase_order_id)
    REFERENCES purchase_orders(id)
    ON DELETE CASCADE
);

CREATE INDEX idx_purchase_order_items_po
ON purchase_order_items(purchase_order_id);

CREATE INDEX idx_purchase_order_items_ingredient
ON purchase_order_items(ingredient_id);

-- -----------------------------------------------------------------------------
-- Table Supplier Payments (Thanh toán đơn nhập hàng)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS supplier_payments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  purchase_order_id UUID NOT NULL,
  code VARCHAR(50) NOT NULL,
  amount DECIMAL(18,2) NOT NULL CHECK (amount > 0),
  method INT NOT NULL DEFAULT 0,              -- 0:Cash, 1:VNPay
  status INT NOT NULL DEFAULT 0,             -- 0:Pending, 1:Paid, 2:Failed
  provider VARCHAR(50),                      -- 'vnpay' hoặc null
  provider_txn_id VARCHAR(100),
  provider_payload TEXT,
  notes TEXT,
  paid_at TIMESTAMP WITH TIME ZONE,
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (purchase_order_id)
    REFERENCES purchase_orders(id)
    ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ux_supplier_payments_code
ON supplier_payments(code)
WHERE is_deleted = FALSE;

CREATE INDEX idx_supplier_payments_po
ON supplier_payments(purchase_order_id);

CREATE INDEX idx_supplier_payments_status
ON supplier_payments(status);
