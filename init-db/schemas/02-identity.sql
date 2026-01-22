-- Extention for gene uuid
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Users table (example)
-- CREATE TABLE IF NOT EXISTS users (
--     id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
--     username VARCHAR(50) UNIQUE NOT NULL,
--     password_hash VARCHAR(255) NOT NULL,
--     full_name VARCHAR(100) NOT NULL,
--     email VARCHAR(100) UNIQUE,
--     phone_number VARCHAR(15),
--     status VARCHAR(20) DEFAULT 'ACTIVE', -- ACTIVE, LOCKED
--     created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
--     is_deleted BOOLEAN DEFAULT FALSE
-- );