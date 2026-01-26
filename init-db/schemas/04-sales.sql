CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code VARCHAR(50) NOT NULL, -- (Human Readable), example: ORD-1234-5678

    subtotal DECIMAL(12,2) NOT NULL,
    tax DECIMAL(12,2) NOT NULL DEFAULT 0,
    total_amount DECIMAL(12,2) NOT NULL,
    paid_amount DECIMAL(12,2),
    
    payment_method INT, -- 0: Cash, 1: VNPay
    status INT NOT NULL DEFAULT 0, -- Pending, Paid, Cancelled
    paid_at TIMESTAMP WITH TIME ZONE,
    notes VARCHAR(255),
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    customer_id UUID
);
CREATE UNIQUE INDEX idx_orders_code ON orders(code);
CREATE INDEX idx_orders_customer_created ON orders(customer_id, created_at DESC) WHERE customer_id IS NOT NULL;

-- Order Items
CREATE TABLE IF NOT EXISTS order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(12,2) NOT NULL,

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