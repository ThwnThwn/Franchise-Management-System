CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
-- 0. Cart Items
CREATE TABLE IF NOT EXISTS cart_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sku VARCHAR(100) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    base_price DECIMAL(12,3) NOT NULL,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    customer_id UUID
);
CREATE UNIQUE INDEX idx_cart_items_customer_sku ON cart_items(customer_id, sku) WHERE customer_id IS NOT NULL;
CREATE INDEX idx_cart_items_customer_created ON cart_items (customer_id ASC, created_at DESC) WHERE customer_id IS NOT NULL;
-- 1. Orders
CREATE TABLE IF NOT EXISTS orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code VARCHAR(50) NOT NULL, 
    subtotal DECIMAL(12,3) NOT NULL,
    tax DECIMAL(12,3) NOT NULL DEFAULT 0,
    total_amount DECIMAL(12,3) NOT NULL,
    paid_amount DECIMAL(12,3),
    
    payment_method INT, -- 0: Cash, 1: VNPay
    status INT NOT NULL DEFAULT 0, -- Pending, Paid, Cancelled
    paid_at TIMESTAMP WITH TIME ZONE,
    notes VARCHAR(255),
    
    type INT, -- 0.Delivery, 1.Pickup
    shipping_fee DECIMAL(12,3) NOT NULL DEFAULT 0,
    address VARCHAR(500),
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    distance DECIMAL(12,3),

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    customer_id UUID,
    store_id UUID,
    delivery_id UUID,
    scheduled_time TIME   
);
CREATE UNIQUE INDEX idx_orders_code ON orders(code);
CREATE INDEX idx_orders_customer_created ON orders(customer_id, created_at DESC) WHERE customer_id IS NOT NULL;

-- 2.1. Order Items
CREATE TABLE IF NOT EXISTS order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(12,3) NOT NULL,

    product_name VARCHAR(255) NOT NULL,
    sku VARCHAR(100) NOT NULL,
    image_url VARCHAR(500), 
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    product_id UUID NOT NULL, 
    variant_id UUID NOT NULL, 
    order_id UUID NOT NULL,
    CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders(id)
);
CREATE INDEX idx_order_items_order ON order_items(order_id);

-- 2.2. Order Sale Items (Discounts / Reductions applied to order)
CREATE TABLE IF NOT EXISTS order_sale_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    amount DECIMAL(12,3) NOT NULL CHECK (amount >= 0),

    type INT NOT NULL, 
    name VARCHAR(255) NOT NULL,
    reference_id UUID,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    order_id UUID NOT NULL,
    CONSTRAINT fk_order_sale_items_order FOREIGN KEY (order_id) REFERENCES orders(id)
);
CREATE INDEX IF NOT EXISTS idx_order_sale_items_order ON order_sale_items(order_id);
CREATE INDEX IF NOT EXISTS idx_order_sale_items_ref ON order_sale_items(reference_id);

-- 2.3 Order Trackings
CREATE TABLE IF NOT EXISTS order_trackings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL,
    status INT,
    action VARCHAR(255) NOT NULL,
    note VARCHAR(500),
    created_by UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_order_trackings_order
        FOREIGN KEY (order_id) REFERENCES orders(id)
);
CREATE INDEX IF NOT EXISTS idx_order_trackings_order ON order_trackings(order_id);

-- 3. Payments
CREATE TABLE IF NOT EXISTS payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID ,
    CONSTRAINT fk_payments_order FOREIGN KEY (order_id) REFERENCES orders(id),
    code VARCHAR(50) NOT NULL, 
    amount DECIMAL(12,3) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'VND',
    -- 0: Cash, 1: VNPay
    method INT NOT NULL,
    -- 0: Pending, 1: Paid, 2: Failed, 3: Refunded, 4: Cancelled
    status INT NOT NULL DEFAULT 0,
    provider VARCHAR(50),           
    provider_txn_id VARCHAR(100),      
    provider_payload JSONB,           
    paid_at TIMESTAMP WITH TIME ZONE,

    reference_id UUID,
    reference_type INT,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE
);
CREATE UNIQUE INDEX idx_payments_code ON payments(code);
CREATE INDEX idx_payments_order_id ON payments(order_id);
CREATE INDEX idx_payments_status ON payments(status);

-- 4. Payment Events
CREATE TABLE IF NOT EXISTS payment_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID NOT NULL,
    CONSTRAINT fk_payment_events_payment FOREIGN KEY (payment_id) REFERENCES payments(id),
    event_type VARCHAR(50) NOT NULL, 
    payload JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_payment_events_payment_id ON payment_events(payment_id);


-- 5. Outbox Messages
CREATE TABLE IF NOT EXISTS outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    occurred_on TIMESTAMP WITH TIME ZONE NOT NULL,
    processed_on TIMESTAMP WITH TIME ZONE,
    error TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_outbox_messages_processed_on ON outbox_messages(processed_on) WHERE processed_on IS NULL;


CREATE TABLE IF NOT EXISTS wallets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    user_id UUID NOT NULL,
    balance NUMERIC(18,2) NOT NULL DEFAULT 0,

    status INT NOT NULL,
    notes VARCHAR(500),

    row_version BYTEA NOT NULL DEFAULT '\x0000000000000001',

    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID NOT NULL,

    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_wallets_user
ON wallets(user_id)
WHERE is_deleted = FALSE;

CREATE INDEX IF NOT EXISTS idx_wallets_status
ON wallets(status);

--  Function tăng row_version mỗi khi UPDATE
CREATE OR REPLACE FUNCTION fn_increment_row_version()
RETURNS TRIGGER AS $$
DECLARE
    current_val bigint;
BEGIN
    current_val := (
        get_byte(OLD.row_version, 0)::bigint << 56 |
        get_byte(OLD.row_version, 1)::bigint << 48 |
        get_byte(OLD.row_version, 2)::bigint << 40 |
        get_byte(OLD.row_version, 3)::bigint << 32 |
        get_byte(OLD.row_version, 4)::bigint << 24 |
        get_byte(OLD.row_version, 5)::bigint << 16 |
        get_byte(OLD.row_version, 6)::bigint << 8  |
        get_byte(OLD.row_version, 7)::bigint
    );

    NEW.row_version = int8send(current_val + 1);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--  Trigger gắn vào wallets
CREATE OR REPLACE TRIGGER trg_wallets_row_version
BEFORE UPDATE ON wallets
FOR EACH ROW
EXECUTE FUNCTION fn_increment_row_version();

CREATE TABLE IF NOT EXISTS wallet_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    wallet_id UUID NOT NULL,
    user_id UUID NOT NULL,
    order_id UUID,
    initiated_by UUID,
    processed_by UUID,
    parent_transaction_id UUID,

    transaction_code VARCHAR(50) NOT NULL,

    amount NUMERIC(18,2) NOT NULL,
    balance_before NUMERIC(18,2),
    balance_after NUMERIC(18,2),

    transaction_type INT NOT NULL,
    status INT NOT NULL,

    reference_id UUID,
    reference_type INT,

    description VARCHAR(500),
    external_transaction_id VARCHAR(100),
    notes VARCHAR(500),
    metadata TEXT,
    failure_reason VARCHAR(500),

    payment_method INT,

    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    failed_at TIMESTAMP WITH TIME ZONE,
    processed_at TIMESTAMP WITH TIME ZONE,

    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_wallet_transactions_wallet
        FOREIGN KEY (wallet_id) REFERENCES wallets(id),

    CONSTRAINT fk_wallet_transactions_parent
        FOREIGN KEY (parent_transaction_id) REFERENCES wallet_transactions(id),

    CONSTRAINT fk_wallet_transactions_order
        FOREIGN KEY (order_id) REFERENCES orders(id)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_wallet_transactions_code
ON wallet_transactions(transaction_code);

CREATE INDEX IF NOT EXISTS idx_wallet_transactions_wallet
ON wallet_transactions(wallet_id, created_at DESC);

CREATE INDEX IF NOT EXISTS idx_wallet_transactions_user
ON wallet_transactions(user_id);

CREATE INDEX IF NOT EXISTS idx_wallet_transactions_status
ON wallet_transactions(status);

CREATE TABLE IF NOT EXISTS refund_tickets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    ticket_code VARCHAR(50) NOT NULL,

    amount NUMERIC(18,2) NOT NULL,
    refund_amount NUMERIC(18,2) NOT NULL,

    reason VARCHAR(500),
    rejection_reason VARCHAR(500),
    rejection_notes VARCHAR(500),

    wallet_id UUID ,
    user_id UUID NOT NULL,
    transaction_id UUID,
    original_transaction_id UUID ,

    status INT NOT NULL,

    processed_by UUID,

    approved_by UUID,
    approval_notes VARCHAR(500),
    approved_at TIMESTAMP WITH TIME ZONE,

    rejected_by UUID,
    rejected_at TIMESTAMP WITH TIME ZONE,

    cancelled_by UUID,
    cancellation_reason VARCHAR(500),
    cancelled_at TIMESTAMP WITH TIME ZONE,

    processed_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,

    refund_reason INT,
    refund_ticket_method INT,

    bank_name VARCHAR(255),
    notes VARCHAR(500),
    internal_notes VARCHAR(500),
    created_by_internal_user_id UUID,

    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_refund_wallet
        FOREIGN KEY (wallet_id) REFERENCES wallets(id),

    CONSTRAINT fk_refund_transaction
        FOREIGN KEY (transaction_id) REFERENCES wallet_transactions(id),

    CONSTRAINT fk_refund_original_transaction
        FOREIGN KEY (original_transaction_id) REFERENCES wallet_transactions(id)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_refund_tickets_code
ON refund_tickets(ticket_code);

CREATE INDEX IF NOT EXISTS idx_refund_tickets_wallet
ON refund_tickets(wallet_id);

CREATE INDEX IF NOT EXISTS idx_refund_tickets_status
ON refund_tickets(status);


CREATE TABLE IF NOT EXISTS wallet_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    wallet_id UUID NOT NULL,
    user_id UUID NOT NULL,
    wallet_transaction_id UUID,

    event_type INT NOT NULL,
    amount NUMERIC(18,2) NOT NULL,

    status INT NOT NULL,

    processed_by_admin_id UUID,
    admin_note VARCHAR(500),

    processed_at TIMESTAMP WITH TIME ZONE,

    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_wallet_events_wallet
        FOREIGN KEY (wallet_id) REFERENCES wallets(id),

    CONSTRAINT fk_wallet_events_wallet_transaction
        FOREIGN KEY (wallet_transaction_id) REFERENCES wallet_transactions(id)
);
CREATE INDEX IF NOT EXISTS idx_wallet_events_wallet
ON wallet_events(wallet_id, created_at DESC);

CREATE INDEX IF NOT EXISTS idx_wallet_events_user
ON wallet_events(user_id);

CREATE INDEX IF NOT EXISTS idx_wallet_events_status
ON wallet_events(status);

CREATE INDEX IF NOT EXISTS idx_wallet_events_type
ON wallet_events(event_type);

CREATE INDEX IF NOT EXISTS idx_wallet_events_transaction
ON wallet_events(wallet_transaction_id);

CREATE TABLE IF NOT EXISTS withdrawal_requests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    -- Mã giao dịch rút tiền (VD: WR-202310-XYZ)
    request_code VARCHAR(50) NOT NULL,

    -- Số tiền khách muốn rút
    amount NUMERIC(18,2) NOT NULL,
    -- (Tùy chọn) Nếu hệ thống thu phí rút tiền thì dùng cột này
    fee_amount NUMERIC(18,2) NOT NULL DEFAULT 0, 

    -- ==========================================
    -- THÔNG TIN NGÂN HÀNG
    -- ==========================================
    bank_code VARCHAR(50),                     -- NULLABLE: Bị rỗng nếu API VietQR lỗi
    bank_name VARCHAR(255) NOT NULL,           -- BẮT BUỘC: Do API trả về HOẶC do khách tự gõ
    account_name VARCHAR(255) NOT NULL,        -- VD: 'NGUYEN VAN A' 
    masked_account_number VARCHAR(50) NOT NULL,-- VD: '******6789' (Lưu plain-text)
    encrypted_account_number TEXT NOT NULL,    -- Chuỗi AES-256 Base64

    -- ==========================================
    -- LIÊN KẾT HỆ THỐNG
    -- ==========================================
    wallet_id UUID NOT NULL,
    user_id UUID NOT NULL,                     -- Chủ nhân của lệnh rút tiền này
    transaction_id UUID,                       -- Trỏ tới mã giao dịch trừ tiền trong bảng wallet_transactions (nếu có)

    -- ==========================================
    -- TRẠNG THÁI & GHI CHÚ
    -- ==========================================
    status INT NOT NULL,                       -- 1: Pending, 2: Approved, 3: Rejected, 4: Cancelled, 5: Completed
    
    notes VARCHAR(500),                        -- Ghi chú của khách hàng khi rút tiền (nếu có)
    internal_notes VARCHAR(500),               -- Ghi chú nội bộ cho Kế toán đọc (Khách không thấy)
    
    rejection_reason VARCHAR(500),             -- Lý do từ chối (Gửi cho khách xem)
    approval_notes VARCHAR(500),               -- Ghi chú lúc duyệt
    cancellation_reason VARCHAR(500),          -- Lý do hủy (Do khách hoặc admin tự hủy)

    -- ==========================================
    -- DẤU VẾT KIỂM TOÁN (AUDIT TRAIL)
    -- ==========================================
    approved_by UUID,
    approved_at TIMESTAMP WITH TIME ZONE,

    rejected_by UUID,
    rejected_at TIMESTAMP WITH TIME ZONE,

    cancelled_by UUID,
    cancelled_at TIMESTAMP WITH TIME ZONE,

    processed_by UUID,                         -- Kế toán viên trực tiếp chuyển khoản
    completed_at TIMESTAMP WITH TIME ZONE,     -- Thời điểm tiền thực sự đi khỏi tài khoản công ty

    -- ==========================================
    -- BẰNG CHỨNG CHUYỂN KHOẢN (RẤT QUAN TRỌNG)
    -- ==========================================
    bank_transaction_id VARCHAR(100),          -- THÊM MỚI: Mã giao dịch/Trace Number từ ngân hàng
    receipt_image_url VARCHAR(1000),           -- Link ảnh bill up
    receipt_image_public_id VARCHAR(255),      -- ID ảnh để sau này có xóa thì gọi API xóa trên Cloudinary

    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- ==========================================
    -- CONSTRAINTS
    -- ==========================================
    CONSTRAINT fk_withdrawal_wallet
        FOREIGN KEY (wallet_id) REFERENCES wallets(id),

    CONSTRAINT fk_withdrawal_transaction
        FOREIGN KEY (transaction_id) REFERENCES wallet_transactions(id)
);

-- Tạo Index để tối ưu tốc độ truy vấn
CREATE UNIQUE INDEX IF NOT EXISTS idx_withdrawal_requests_code 
ON withdrawal_requests(request_code);

CREATE INDEX IF NOT EXISTS idx_withdrawal_requests_wallet 
ON withdrawal_requests(wallet_id);

CREATE INDEX IF NOT EXISTS idx_withdrawal_requests_user 
ON withdrawal_requests(user_id);

CREATE INDEX IF NOT EXISTS idx_withdrawal_requests_status 
ON withdrawal_requests(status);

CREATE INDEX IF NOT EXISTS idx_withdrawal_requests_created_at 
ON withdrawal_requests(created_at DESC);