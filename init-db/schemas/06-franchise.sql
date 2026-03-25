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
    start_time TIME NOT NULL, 
    end_time TIME NOT NULL,
    
    required_staff_count INT NOT NULL DEFAULT 1,
    
    shift_template_id UUID,
    
    -- Check status để cho phép Edit/Delete
    -- 0: Draft/Planning
    -- 1: Published (Nhân viên thấy được để đăng ký hoặc xem)
    -- 2: Completed (Đã diễn ra xong)
    -- 3: Cancelled
    status INT DEFAULT 0,
    
    note TEXT,
    
    break_duration_minutes INT DEFAULT 0, -- Thời gian nghỉ (phút)
    lock_version INT DEFAULT 1,           -- Optimistic Locking
    
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

    is_overridden BOOLEAN DEFAULT FALSE,
    override_reason TEXT,
    overridden_by UUID,

    
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

    grace_period_minutes INT DEFAULT 5,
    daily_ot_threshold_hours INT DEFAULT 8,
    weekly_ot_threshold_hours INT DEFAULT 40,

    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX idx_rules_store_unique ON shift_rules(store_id);

-- ========================================================
-- 8. STAFF AVAILABILITIES (Lịch của nhân viên)
-- ========================================================
CREATE TABLE IF NOT EXISTS staff_availabilities (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    
    -- Trạng thái: TRUE (Rảnh), FALSE (Bận)
    is_available BOOLEAN NOT NULL DEFAULT TRUE,

    -- TRUE: Lặp lại hàng tuần (Thứ 2, 3...) | FALSE: Ngày cụ thể (Việc đột xuất)
    is_recurring BOOLEAN DEFAULT FALSE,

    -- Dùng khi is_recurring = FALSE (Ví dụ: '2026-02-14')
    specific_date DATE, 

    -- Dùng khi is_recurring = TRUE (0: Chủ Nhật, ..., 6: Thứ Bảy)
    day_of_week SMALLINT CHECK (day_of_week BETWEEN 0 AND 6),
    
    -- Giờ bắt đầu (Có thể > end_time nếu làm qua đêm)
    start_time TIME NOT NULL, 
    -- Giờ kết thúc
    end_time TIME NOT NULL,
    
    -- Lý do bận hoặc ghi chú rảnh (nullable)
    note VARCHAR(255),
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP, 
    is_deleted BOOLEAN DEFAULT FALSE,

    -- Ràng buộc logic dữ liệu hybrid
    CONSTRAINT date_or_day_check CHECK (
        (is_recurring = FALSE AND specific_date IS NOT NULL) OR 
        (is_recurring = TRUE AND day_of_week IS NOT NULL)
    )
);

CREATE INDEX idx_staff_availability_query 
ON staff_availabilities(user_id, is_available, specific_date, day_of_week) 
WHERE is_deleted = FALSE;

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
    -- Số phút làm việc thường và tăng ca (Overtime)
    regular_minutes INT DEFAULT 0,
    daily_ot_minutes INT DEFAULT 0,
    weekly_ot_minutes INT DEFAULT 0,
    check_out_lat DECIMAL(10,8),
    check_out_long DECIMAL(11,8),
    
    -- 0: Processing, 1: Valid, 2: Invalid
    status INT DEFAULT 0,
    manager_note TEXT,      -- Ghi chú của quản lý (lý do sửa công, duyệt trễ...)
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    CONSTRAINT fk_timelogs_assignment FOREIGN KEY (shift_assignment_id) REFERENCES shift_assignments(id)
);
CREATE INDEX idx_timelogs_assignment ON time_logs(shift_assignment_id);

-- =======================================================
-- 10. OUTBOX MESSAGES (Pattern for Reliable Events)
-- =======================================================
CREATE TABLE IF NOT EXISTS outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type TEXT NOT NULL,
    content TEXT NOT NULL,
    occurred_on TIMESTAMP WITH TIME ZONE NOT NULL,
    processed_on TIMESTAMP WITH TIME ZONE NULL,
    error TEXT NULL,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_outbox_messages_processed_on ON outbox_messages(processed_on) WHERE processed_on IS NULL;

-- ========================================================
-- 11. SHIFT APPLICATIONS (Don dang ky nhan ca tu Cho Ca)
-- Nhan vien thay ca dang mo (Open) tren App, bam "Dang ky"
-- Manager vao Web xem danh sach don roi Duyet hoac Tu choi
-- ========================================================
CREATE TABLE IF NOT EXISTS shift_applications (
    -- Khoa chinh
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    -- Ca lam viec ma nhan vien muon dang ky nhan
    shift_id UUID NOT NULL,

    -- ID nhan vien gui don dang ky (tu Identity Service)
    user_id UUID NOT NULL,

    -- Trang thai don dang ky:
    -- 0: Pending  (Dang cho Manager duyet)
    -- 1: Approved (Manager da duyet, tu dong tao ShiftAssignment)
    -- 2: Rejected (Manager tu choi)
    status INT DEFAULT 0,

    -- Ghi chu cua nhan vien khi dang ky (vi du: "Em ranh ca nay, xin nhan")
    note TEXT,

    -- Thoi diem nhan vien bam nut "Dang ky nhan ca"
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    -- Thoi diem Manager duyet hoac tu choi don
    reviewed_at TIMESTAMP WITH TIME ZONE,

    -- ID cua Manager da duyet/tu choi don nay
    reviewed_by UUID,

    -- Ghi chu cua Manager khi duyet/tu choi (vi du: "Tu choi vi da du nguoi")
    review_note TEXT,

    -- Truong audit chuan
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,

    -- Rang buoc: don phai thuoc ve 1 ca ton tai
    CONSTRAINT fk_applications_shift FOREIGN KEY (shift_id) REFERENCES shifts(id),

    -- Rang buoc: 1 nhan vien chi duoc dang ky 1 lan cho moi ca
    CONSTRAINT uq_application_user_shift UNIQUE (shift_id, user_id)
);

-- Index de truy van nhanh theo ca (Manager xem danh sach ung vien cua 1 ca)
CREATE INDEX idx_applications_shift ON shift_applications(shift_id);

-- Index de truy van nhanh theo nhan vien (xem lich su dang ky cua 1 nguoi)
CREATE INDEX idx_applications_user ON shift_applications(user_id);

-- ========================================================
-- 10. SHIFT SWAP REQUESTS (Yeu cau doi ca)
-- ========================================================
CREATE TABLE IF NOT EXISTS shift_swap_requests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    -- A: nguoi gui yeu cau doi ca
    source_assignment_id UUID NOT NULL,
    source_user_id UUID NOT NULL,

    -- B: nguoi duoc yeu cau nhan ca
    target_user_id UUID NOT NULL,

    -- Trang thai State Machine (0=PendingTarget, 1=PendingManager, 2=Approved, 3=Rejected, 4=Expired, 5=Cancelled)
    status INT NOT NULL DEFAULT 0,

    -- Ly do gui yeu cau (bat buoc)
    reason VARCHAR(500) NOT NULL,

    -- Ly do tu choi
    reject_reason VARCHAR(500),

    -- Thoi han B phai tra loi
    expires_at TIMESTAMPTZ NOT NULL,

    -- Ai duyet (Manager)
    reviewed_by UUID,
    reviewed_at TIMESTAMPTZ,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_swap_source_assignment FOREIGN KEY (source_assignment_id) REFERENCES shift_assignments(id)
);

CREATE INDEX idx_swap_source_user ON shift_swap_requests(source_user_id, status);
CREATE INDEX idx_swap_target_user ON shift_swap_requests(target_user_id, status);

-- ========================================================
-- 11. FRANCHISE APPLICATIONS (Hồ sơ đăng ký nhượng quyền)
-- ========================================================
CREATE TABLE IF NOT EXISTS franchise_applications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    store_id  UUID NOT NULL,
    manager_id UUID NOT NULL, 
    
    -- Thông tin cá nhân đối tác
    full_name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    identity_number VARCHAR(20) NOT NULL, 
    
    -- Thông tin mặt bằng & Pháp lý
    province_id VARCHAR(50),
    district_id VARCHAR(50),
    ward_id VARCHAR(50),
    store_name VARCHAR(50),
    store_address TEXT NOT NULL,
    store_latitude DECIMAL(10, 8),
    store_longitude DECIMAL(11, 8),
    store_size_sqm DECIMAL(10, 2),
    
    -- Lưu trữ trên Cloudinary cho Giấy chứng nhận đất đai
    -- Dùng mảng TEXT[] để lưu nhiều giấy tờ, sổ đỏ/hồng
    land_certificates TEXT[], 
    
    -- Lưu trữ trên Cloudinary cho Ảnh mặt bằng
    store_front_url VARCHAR(500),
    store_front_public_id VARCHAR(100),
    
    -- Trạng thái phê duyệt
    -- 0: Pending, 1: Approved, 2: Rejected
    status INT DEFAULT 0, 
    admin_note TEXT,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    reviewed_at TIMESTAMP WITH TIME ZONE,
    reviewed_by UUID
);

-- 1. Lọc theo trạng thái (Admin Dashboard)
CREATE INDEX idx_franchise_app_status ON franchise_applications(status);

-- 2. Tìm kiếm theo thông tin cá nhân (Email & CCCD)
CREATE INDEX idx_franchise_app_email ON franchise_applications(email);
CREATE INDEX idx_franchise_app_identity ON franchise_applications(identity_number);

-- 3. Tìm kiếm theo người duyệt (Admin)
CREATE INDEX idx_franchise_app_reviewed_by ON franchise_applications(reviewed_by);


-- ========================================================
-- 12. FRANCHISE CONTRACTS (Hợp đồng nhượng quyền)
-- ========================================================
CREATE TABLE IF NOT EXISTS franchise_contracts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code VARCHAR(100) NOT NULL,
    application_id UUID NOT NULL,
    
    -- Liên kết DocuSeal
    docuseal_submission_id INT,    -- Submission ID từ Docuseal
    contract_pdf_url VARCHAR(500), -- Signed URL file hợp đồng (S3/Cloudinary)
    
    -- Thông số tài chính cơ bản
    deposit_amount DECIMAL(15, 2),
    deposit_status INT DEFAULT 0,  -- 0: Chưa thanh toán (Awaiting_Payment), 1: Đã thanh toán (Paid)
    
    franchise_fee DECIMAL(15, 2),
    revenue_share_percentage DECIMAL(5, 2),
    
    -- Trạng thái Hợp đồng
    -- 0: Draft, 1: Approved, 2: Awaiting_Signature, 3: Awaiting_Payment, 4: Active, 5: Suspended, 6: Terminated, 7: Expired
    status INT DEFAULT 0,
    
    valid_from TIMESTAMP WITH TIME ZONE,
    valid_until TIMESTAMP WITH TIME ZONE,
    
    -- Quản lý Tiến độ thi công (Store Onboarding)
    -- 0: Thiết kế (Design), 1: Thi công (Construction), 2: Nghiệm thu (Inspection), 3: Khai trương (Ready)
    setup_status INT DEFAULT 0,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT fk_contract_application FOREIGN KEY (application_id) REFERENCES franchise_applications(id)
);

-- Index của Hợp đồng
CREATE INDEX idx_franchise_contract_app ON franchise_contracts(application_id);
-- Cho Webhook xử lý DocuSeal:
CREATE INDEX idx_franchise_contract_docuseal ON franchise_contracts(docuseal_submission_id);
CREATE INDEX idx_franchise_contract_status ON franchise_contracts(status);