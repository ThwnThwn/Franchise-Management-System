CREATE TABLE IF NOT EXISTS product_variants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL, -- logical reference, NO FK
    sku VARCHAR(100) NOT NULL UNIQUE,
    variant_name VARCHAR(255),
    attributes JSONB,
    base_price DECIMAL(12,2) NOT NULL,
    cost DECIMAL(12,2) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
