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
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    customer_id UUID,
    store_id UUID
);
CREATE UNIQUE INDEX idx_orders_code ON orders(code);
CREATE INDEX idx_orders_customer_created ON orders(customer_id, created_at DESC) WHERE customer_id IS NOT NULL;

-- 2. Order Items
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

-- 3. Payments
CREATE TABLE IF NOT EXISTS payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL,
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
    balance DECIMAL(18,2) NOT NULL,

    status INT NOT NULL,          -- 0=Inactive,1=Active,2=Locked
    row_version BYTEA,            -- optimistic concurrency

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    user_id UUID NOT NULL
);

CREATE UNIQUE INDEX idx_wallets_user ON wallets(user_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_wallets_status ON wallets(status);


CREATE TABLE IF NOT EXISTS wallet_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    CONSTRAINT fk_wallet_transactions_wallet
        FOREIGN KEY (wallet_id) REFERENCES wallets(id),

    code VARCHAR(50) NOT NULL,      -- TXN-20260203-000001
    amount DECIMAL(18,2) NOT NULL,

    balance_before DECIMAL(18,2) NOT NULL,
    balance_after DECIMAL(18,2) NOT NULL,

    transaction_type INT NOT NULL,  -- Deposit, Payment, Refund...
    status INT NOT NULL,            -- Pending, Completed, Failed

    reference_id UUID,
    reference_type VARCHAR(50),     -- Order, RefundTicket...

    description VARCHAR(500),

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN DEFAULT FALSE,

    wallet_id UUID NOT NULL
);

CREATE UNIQUE INDEX idx_wallet_transactions_code
ON wallet_transactions(code);

CREATE INDEX idx_wallet_transactions_wallet
ON wallet_transactions(wallet_id, created_at DESC);

CREATE INDEX idx_wallet_transactions_reference ON wallet_transactions(reference_id) 
WHERE reference_id IS NOT NULL;

CREATE INDEX idx_wallet_transactions_status ON wallet_transactions(status);


CREATE TABLE IF NOT EXISTS refund_tickets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    code VARCHAR(50) NOT NULL,      -- REFUND-20260203-000001
    amount DECIMAL(18,2) NOT NULL,
    reason VARCHAR(500),

    status INT NOT NULL,            -- Pending, Approved, Rejected...

    processed_by UUID,
    rejection_reason VARCHAR(500),

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN DEFAULT FALSE,

    wallet_id UUID NOT NULL,
    transaction_id UUID,
    order_id UUID,
    original_transaction_id UUID,

    CONSTRAINT fk_refund_tickets_wallet
        FOREIGN KEY (wallet_id) REFERENCES wallets(id),

    CONSTRAINT fk_refund_tickets_transaction
        FOREIGN KEY (transaction_id) REFERENCES wallet_transactions(id)
);

CREATE UNIQUE INDEX idx_refund_tickets_code ON refund_tickets(code);

CREATE INDEX idx_refund_tickets_wallet ON refund_tickets(wallet_id, created_at DESC);

CREATE INDEX idx_refund_tickets_status ON refund_tickets(status);

CREATE INDEX idx_refund_tickets_transaction ON refund_tickets(transaction_id)
WHERE transaction_id IS NOT NULL;