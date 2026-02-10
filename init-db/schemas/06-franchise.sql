CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ========================================================
-- 1. FRANCHISEES (Đối tác nhượng quyền) 
-- ========================================================
CREATE TABLE IF NOT EXISTS franchisees (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Business Info
    name VARCHAR(255) NOT NULL,
    tax_code VARCHAR(50),            
    representative_name VARCHAR(100), 
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255) NOT NULL,
    
    -- Financial Info 
    bank_name VARCHAR(100),
    bank_account_number VARCHAR(50),
    bank_account_holder VARCHAR(100),
    
    -- Contract Info
    contract_start_date TIMESTAMP WITH TIME ZONE,
    contract_end_date TIMESTAMP WITH TIME ZONE,
    commission_rate DECIMAL(5,2) DEFAULT 0, 
    
    is_active BOOLEAN DEFAULT TRUE,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE
);

-- ========================================================
-- 2. STORES (Cửa hàng vật lý) 
-- ========================================================
CREATE TABLE IF NOT EXISTS stores (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Display Info
    code VARCHAR(50) NOT NULL, 
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(255), 
    image_url VARCHAR(500),
    
    -- Location (Geo-Search)
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    
    -- Operations
    type INT NOT NULL DEFAULT 0,   -- 0: Company Owned, 1: Franchise
    status INT NOT NULL DEFAULT 1, -- 1: Open, 2: Closed, 3: Renovating
    
    franchisee_id UUID,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_stores_franchisee FOREIGN KEY (franchisee_id) REFERENCES franchisees(id)
);

CREATE UNIQUE INDEX idx_stores_code ON stores(code);
CREATE INDEX idx_stores_location ON stores(latitude, longitude);
CREATE INDEX idx_stores_franchisee ON stores(franchisee_id);


-- ========================================================
-- 3. STORE HOURS (Giờ hoạt động) 
-- ========================================================
CREATE TABLE IF NOT EXISTS store_hours (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    store_id UUID NOT NULL,
    
    day_of_week INT NOT NULL, -- 0=Sun, 1=Mon...
    open_time TIME NOT NULL,
    close_time TIME NOT NULL,
    
    is_closed BOOLEAN DEFAULT FALSE, 

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_hours_store FOREIGN KEY (store_id) REFERENCES stores(id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX idx_hours_store_unique ON store_hours(store_id, day_of_week);


-- ========================================================
-- 4. SHIFT TEMPLATES (Mẫu ca làm việc)
-- ========================================================
CREATE TABLE IF NOT EXISTS shift_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    store_id UUID, 
    
    code VARCHAR(50) NOT NULL,  -- MORNING
    name VARCHAR(100) NOT NULL, -- Ca Sáng
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    break_duration_minutes INT DEFAULT 0, 
    
    is_active BOOLEAN DEFAULT TRUE,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE
);

-- ========================================================
-- 5. SHIFTS (Cấu hình ca làm việc)
-- ========================================================
CREATE TABLE IF NOT EXISTS shifts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    store_id UUID NOT NULL,
    
    shift_date DATE NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL, 
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    
    required_staff_count INT NOT NULL DEFAULT 1,
    
    shift_template_id UUID,
    
    -- Check status để cho phép Edit/Delete
    -- 0: Draft/Planning
    -- 1: Published (Nhân viên thấy được để đăng ký hoặc xem)
    -- 2: Completed (Đã diễn ra xong)
    -- 3: Cancelled
    status INT DEFAULT 0,
    
    note TEXT,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_shifts_store FOREIGN KEY (store_id) REFERENCES stores(id)
);

-- Index cho (View Schedule List)
CREATE INDEX idx_shifts_store_date ON shifts(store_id, shift_date);


-- ========================================================
-- 6. SHIFT ASSIGNMENTS (Phân công nhân viên)
-- ========================================================
CREATE TABLE IF NOT EXISTS shift_assignments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    shift_id UUID NOT NULL,
    user_id UUID NOT NULL,
    
    -- Availability Status có thể check ở bảng khác, nhưng ở đây lưu trạng thái gán
    -- 0: Assigned (Quản lý gán)
    -- 1: Confirmed (Nhân viên xác nhận - Optional)
    -- 2: Rejected (Nhân viên từ chối - Optional)
    status INT DEFAULT 0,
    
    -- Lưu lại wage tại thời điểm gán (snapshot)
    hourly_wage DECIMAL(10, 2),
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_assignments_shift FOREIGN KEY (shift_id) REFERENCES shifts(id),
    -- Constraint: 1 nhân viên không được gán 2 lần vào cùng 1 ca
    CONSTRAINT uq_assignment_user_shift UNIQUE (shift_id, user_id)
);

-- Index để tìm "Lịch làm của tôi"
CREATE INDEX idx_assignments_user ON shift_assignments(user_id);


-- ========================================================
-- 7. SHIFT RULES (Luật lệ ca làm)
-- ========================================================
CREATE TABLE IF NOT EXISTS shift_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    store_id UUID, -- NULL = Global Rule, Có ID = Store Specific Rule
    
    -- Story 4: Shuttle & Swap Policies
    allow_shuttle BOOLEAN DEFAULT FALSE,      -- Cho phép nhân viên chạy nhiều shop
    allow_shift_swap BOOLEAN DEFAULT TRUE,    -- Cho phép đổi ca
    
    -- Các rule mở rộng khác
    min_hours_between_shifts INT DEFAULT 8,   -- Nghỉ tối thiểu 8h giữa 2 ca
    max_hours_per_week INT DEFAULT 48,        -- Tối đa giờ làm/tuần
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);


-- ========================================================
-- 8. STAFF AVAILABILITIES (Lịch rảnh của nhân viên)
-- ========================================================
CREATE TABLE IF NOT EXISTS staff_availabilities (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    
    availability_date DATE NOT NULL,
    
    start_time TIME NOT NULL, 
    end_time TIME NOT NULL,
    
    -- Trạng thái: TRUE (Rảnh, có thể làm), FALSE (Bận/Xin nghỉ)
    is_available BOOLEAN DEFAULT TRUE,
    
    note VARCHAR(255),
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP, 
    is_deleted BOOLEAN DEFAULT FALSE
);
CREATE INDEX idx_availabilities_user_date ON staff_availabilities(user_id, availability_date);


-- ========================================================
-- 9. TIME LOGS (Chấm công thực tế)
-- ========================================================
CREATE TABLE IF NOT EXISTS time_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    shift_assignment_id UUID NOT NULL, 
    
    -- Check-in
    check_in_at TIMESTAMP WITH TIME ZONE,
    check_in_image_url VARCHAR(500),
    check_in_lat DECIMAL(10,8),
    check_in_long DECIMAL(11,8),
    
    -- Check-out
    check_out_at TIMESTAMP WITH TIME ZONE,
    check_out_image_url VARCHAR(500),
    check_out_lat DECIMAL(10,8),
    check_out_long DECIMAL(11,8),
    
    -- Result
    total_work_minutes INT DEFAULT 0,
    
    -- 0: Processing, 1: Valid, 2: Invalid
    status INT DEFAULT 0,
    manager_note TEXT,      -- Ghi chú của quản lý (lý do sửa công, duyệt trễ...)
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    CONSTRAINT fk_timelogs_assignment FOREIGN KEY (shift_assignment_id) REFERENCES shift_assignments(id)
);
CREATE INDEX idx_timelogs_assignment ON time_logs(shift_assignment_id);