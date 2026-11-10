-- Extention for gene uuid
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- TABLE ROLES
CREATE TABLE IF NOT EXISTS roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(50) NOT NULL,
    slug VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE
);

-- TABLE PERMISSIONS
CREATE TABLE IF NOT EXISTS permissions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    resource VARCHAR(50) NOT NULL, -- "User", "Product"...
    description TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE, 
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- TABLE ROLE_PERMISSIONS (Bảng Nối - Tối ưu hóa)
CREATE TABLE IF NOT EXISTS role_permissions (
    role_id UUID NOT NULL,
    permission_id UUID NOT NULL,

    PRIMARY KEY (role_id, permission_id),

    CONSTRAINT fk_rp_role 
        FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
        
    CONSTRAINT fk_rp_permission 
        FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
);
-- Giúp query ngược: Tìm xem "Quyền User.Create đang thuộc về những Role nào?"
CREATE INDEX idx_rp_permission_id ON role_permissions(permission_id);

-- TABLE USERS
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR(255),
    email VARCHAR(255) NOT NULL UNIQUE,
    username VARCHAR(70),
    password VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    address TEXT,
    gender INT NOT NULL DEFAULT 0, -- 0: Male, 1: Female
    date_of_birth TIMESTAMP NOT NULL,
    is_blocked BOOLEAN DEFAULT FALSE,
    
    current_hourly_wage DECIMAL(18, 2), -- Mức lương theo giờ hiện tại

    -- Định danh nhân viên này thuộc biên chế cửa hàng nào
    home_store_id UUID,

    avatar_url VARCHAR(500),
    avatar_public_id VARCHAR(255),

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    role_id UUID,
    CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE SET NULL
);
CREATE INDEX idx_users_home_store ON users(home_store_id) WHERE is_deleted = FALSE;

-- TABLE TOKEN
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    token TEXT NOT NULL UNIQUE,
    issued_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_revoked BOOLEAN DEFAULT FALSE,

    user_id UUID NOT NULL,
    CONSTRAINT fk_tokens_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);