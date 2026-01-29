CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ========================================================
-- 1. FRANCHISEES (Đối tác nhượng quyền)
-- ========================================================
CREATE TABLE IF NOT EXISTS franchisees (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Business Info
    name VARCHAR(255) NOT NULL,
    tax_code VARCHAR(50),             -- Mã số thuế
    representative_name VARCHAR(100), -- Người đại diện pháp luật
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255) NOT NULL,
    
    -- Financial Info (Để trả tiền hoa hồng/doanh thu)
    bank_name VARCHAR(100),
    bank_account_number VARCHAR(50),
    bank_account_holder VARCHAR(100),
    
    -- Contract Info
    contract_start_date TIMESTAMP WITH TIME ZONE,
    contract_end_date TIMESTAMP WITH TIME ZONE,
    commission_rate DECIMAL(5,2) DEFAULT 0, -- % Phí trả về HQ
    
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
    code VARCHAR(50) NOT NULL, -- STR-001
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(255), -- Email riêng của cửa hàng (để nhận noti đơn hàng)
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

-- --- INDEXING STRATEGY (STORES) ---
-- 1. Mã cửa hàng là duy nhất
CREATE UNIQUE INDEX idx_stores_code ON stores(code);
-- 2. Tìm cửa hàng gần nhất (Bounding Box Search)
CREATE INDEX idx_stores_location ON stores(latitude, longitude);
-- 3. Lọc cửa hàng theo đối tác 
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
    
    is_closed BOOLEAN DEFAULT FALSE, -- Nghỉ định kỳ

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_hours_store FOREIGN KEY (store_id) REFERENCES stores(id) ON DELETE CASCADE
);
-- Đảm bảo 1 cửa hàng chỉ có 1 cấu hình cho 1 thứ
CREATE UNIQUE INDEX idx_hours_store_unique ON store_hours(store_id, day_of_week);


-- ========================================================
-- 4. SHIFT TEMPLATES (Mẫu ca làm việc)
-- ========================================================
CREATE TABLE IF NOT EXISTS shift_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    store_id UUID, -- Nếu NULL thì là mẫu chung toàn hệ thống, có ID thì là mẫu riêng của quán
    
    code VARCHAR(50) NOT NULL,  -- MORNING
    name VARCHAR(100) NOT NULL, -- Ca Sáng
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    break_duration_minutes INT DEFAULT 0, -- Nghỉ 30p
    
    is_active BOOLEAN DEFAULT TRUE,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE
);


-- ========================================================
-- 5. SCHEDULES (Lịch làm việc - Roster)
-- ========================================================
CREATE TABLE IF NOT EXISTS schedules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Relations
    user_id UUID NOT NULL,  -- Tham chiếu Identity Service
    store_id UUID NOT NULL, -- Làm ở đâu
    shift_template_id UUID, -- Áp dụng mẫu nào (có thể null nếu ca custom)
    
    -- Timing
    shift_date DATE NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL, -- 2026-01-29 07:00:00+07
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,   -- 2026-01-29 12:00:00+07
    break_duration_minutes INT DEFAULT 0,
    
    -- Wage Estimation (Dự tính lương - Optional)
    hourly_wage DECIMAL(10, 2), -- Lương theo giờ dự kiến (lấy tại thời điểm xếp lịch)
    
    -- Meta
    status INT DEFAULT 1, -- 0: Draft, 1: Published, 2: Cancelled
    note TEXT,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT fk_schedules_store FOREIGN KEY (store_id) REFERENCES stores(id)
);

-- --- INDEXING STRATEGY (SCHEDULES) ---
-- 1. App Nhân viên: "Lấy lịch làm việc tháng này của tôi"
CREATE INDEX idx_schedules_user_date ON schedules(user_id, shift_date);
-- 2. App Quản lý: "Xem hôm nay cửa hàng có ai làm?"
CREATE INDEX idx_schedules_store_date ON schedules(store_id, shift_date);
-- 3. Validation: Check trùng lịch (tránh 1 người làm 2 nơi cùng lúc)
CREATE INDEX idx_schedules_overlap ON schedules(user_id, start_time, end_time);


-- ========================================================
-- 6. TIME LOGS (Chấm công thực tế)
-- ========================================================
CREATE TABLE IF NOT EXISTS time_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    schedule_id UUID NOT NULL, -- Link với lịch
    
    -- Check-in
    check_in_at TIMESTAMP WITH TIME ZONE,
    check_in_image_url VARCHAR(500), -- Ảnh FaceID
    check_in_lat DECIMAL(10,8),      -- GPS
    check_in_long DECIMAL(11,8),
    
    -- Check-out
    check_out_at TIMESTAMP WITH TIME ZONE,
    check_out_image_url VARCHAR(500),
    check_out_lat DECIMAL(10,8),
    check_out_long DECIMAL(11,8),
    
    -- Result
    total_work_minutes INT DEFAULT 0, -- (Out - In) - Break
    
    -- Trạng thái chấm công (Khác trạng thái lịch nhé)
    -- 0: Processing (Đang làm/Chưa checkout)
    -- 1: Valid (Hợp lệ - Đã xong)
    -- 2: Invalid (Bất thường - Ví dụ: Quên checkout, checkin sai vị trí)
    status INT DEFAULT 0,
    admin_note TEXT,      -- Ghi chú của quản lý nếu sửa công
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    CONSTRAINT fk_timelogs_schedule FOREIGN KEY (schedule_id) REFERENCES schedules(id)
);
-- Index tìm log theo lịch
CREATE INDEX idx_timelogs_schedule ON time_logs(schedule_id);