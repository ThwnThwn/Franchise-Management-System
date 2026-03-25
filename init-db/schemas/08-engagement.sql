CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 0. ĐỊNH NGHĨA CÁC KIỂU ENUM
-- ---------------------------------------------------------
CREATE TYPE point_action_type AS ENUM ('REDEEM', 'EARN', 'REFUND', 'ADJUST');
CREATE TYPE point_reason_code AS ENUM (
    -- Nhóm cộng điểm
    'ORDER_COMPLETED',      -- Tích điểm từ đơn hàng
    'BIRTHDAY_GIFT',       -- Thưởng sinh nhật
    'PROMOTION_BONUS',     -- Thưởng khuyến mãi
    'REFERRAL_REWARD',     -- Thưởng giới thiệu bạn mới
    
    -- Nhóm trừ điểm
    'POINT_EXPIRED',       -- Điểm hết hạn
    'POINT_REDEEM',
    -- 'YEARLY_ROLLOVER',
    -- 'PERIODIC_EXPIRATION',
    
    -- Nhóm hoàn/hủy/điều chỉnh
    'ORDER_CANCEL_REFUND', -- Hoàn điểm khi hủy đơn
    'ORDER_RETURN_DEDUCT', -- Thu hồi điểm khi trả hàng
    'MANUAL_ADJUST'        -- Điều chỉnh thủ công bởi admin
);
CREATE TYPE voucher_type AS ENUM ('DISCOUNT_PERCENTAGE',
                                    'DISCOUNT_FIXED_AMOUNT',
                                    'SHIPPING_PERCENTAGE', 
                                    'SHIPPING_FIXED_AMOUNT',
                                    'GIFT');
CREATE TYPE user_voucher_status AS ENUM ('AVAILABLE', 'USED', 'EXPIRED');
CREATE TYPE voucher_item_type AS ENUM ('ITEM_GIFT', 'ITEM_REQUIRED');

-- NHÓM 1: QUẢN LÝ HẠNG THÀNH VIÊN & ĐIỂM THƯỞNG
-- ---------------------------------------------------------

CREATE TABLE "membership_tiers" (
    "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "name" VARCHAR(20) NOT NULL,
    "min_spend_required" DECIMAL(15, 3) CHECK ("min_spend_required" >= 0),
    "amount_per_point" DECIMAL(15, 3) CHECK ("amount_per_point" >= 0),
    "value_per_point" DECIMAL(15, 3) CHECK ("value_per_point" >= 0),
    "discount_rate" FLOAT DEFAULT 0.0,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE
);

-- Bảng cấu hình ngày hết hạn tập trung
-- CREATE TABLE "point_expiration_policies" (
--     "applicable_year" INT PRIMARY KEY,           -- Ví dụ: 2025
--     "expiration_date" TIMESTAMP WITH TIME ZONE NOT NULL, -- Ví dụ: 2026-03-31
--     "note" TEXT,
--     "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
-- );

CREATE TABLE "loyalty_accounts" (
    "id" UUID PRIMARY KEY, -- Thường khớp với UserID từ Service Identity
    "available_points" DECIMAL(15, 3) DEFAULT 0 CHECK ("available_points" >= 0),
    
    -- Chia điểm thành 2 giỏ để quản lý hết hạn theo kỳ
    -- "points_this_year" DECIMAL(15, 3) DEFAULT 0 CHECK ("points_this_year" >= 0),
    -- "points_last_year" DECIMAL(15, 3) DEFAULT 0 CHECK ("points_last_year" >= 0),
    
    "total_spent" DECIMAL(15, 3) DEFAULT 0,
    "current_tier_id" UUID REFERENCES "membership_tiers"("id"),
    -- "last_rollover_at" TIMESTAMP WITH TIME ZONE, -- Ngày cuối cùng chuyển điểm từ năm nay sang năm ngoái
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE
);

CREATE TABLE "point_logs" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "user_id" UUID REFERENCES "loyalty_accounts"("id"),
    "amount" DECIMAL(15, 3) NOT NULL,
    "balance_before" DECIMAL(15, 3),
    "balance_after" DECIMAL(15, 3),
    "action_type" point_action_type NOT NULL,
    "reason_code" point_reason_code,
    "reference_id" UUID, -- ID của Order hoặc Voucher liên quan
    "description" TEXT,
    "expired_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "point_rules" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "reason_code" point_reason_code NOT NULL,
    "expiry_days" INT NOT NULL DEFAULT 6,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    -- Quan trọng: Đảm bảo mỗi mã lý do chỉ có một cấu hình duy nhất
    CONSTRAINT "uq_point_rule_reason" UNIQUE ("reason_code")
);

-- NHÓM 2: QUẢN LÝ VOUCHER & KHUYẾN MÃI
-- ---------------------------------------------------------

CREATE TABLE "campaigns" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "name" VARCHAR(255) NOT NULL,
    "description" TEXT,
    "start_date" TIMESTAMP WITH TIME ZONE NOT NULL,
    "end_date" TIMESTAMP WITH TIME ZONE NOT NULL,
    "is_active" BOOLEAN DEFAULT TRUE,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "check_campaign_time" CHECK ("end_date" > "start_date")
);

CREATE TABLE "vouchers" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "campaign_id" UUID REFERENCES "campaigns"("id") ON DELETE SET NULL,
    
    "code" VARCHAR(50) NOT NULL,
    "type" voucher_type NOT NULL,
    "discount_value" DECIMAL(15, 3) CHECK ("discount_value" >= 0),
    "max_discount_value" DECIMAL(15, 3) CHECK ("max_discount_value" >= 0),
    "min_order_value" DECIMAL(15, 3) DEFAULT 0 CHECK ("min_order_value" >= 0),

    "point_cost" DECIMAL(15, 3) DEFAULT 0 CHECK ("point_cost" >= 0),
    "init_quantity" INT,
    "current_quantity" INT,
    "reserved_quantity" INT,
    "max_usage_per_user" INT, 

    "is_active" BOOLEAN DEFAULT TRUE,
    "start_date" TIMESTAMP WITH TIME ZONE,
    "end_date" TIMESTAMP WITH TIME ZONE,

    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE,

    CONSTRAINT "check_quantity_logic" CHECK ("current_quantity" <= "init_quantity")
    -- CONSTRAINT "check_voucher_logic" CHECK (
    --     (
    --         "campaign_id" IS NOT NULL 
    --         AND "start_date" IS NULL 
    --         AND "end_date" IS NULL
    --     )
    --     OR
    --     (
    --         "campaign_id" IS NULL
    --     )
    -- )
);

CREATE UNIQUE INDEX "uq_voucher_code_active" ON "vouchers" ("code") WHERE ("is_deleted" IS FALSE);

CREATE TABLE "user_vouchers" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "user_id" UUID NOT NULL REFERENCES "loyalty_accounts"("id"),
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id"),
    "status" user_voucher_status DEFAULT 'AVAILABLE',
    "acquired_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    -- "expires_at" TIMESTAMP WITH TIME ZONE,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE
);

-- Đã đổi tên bảng thành voucher_items để đồng bộ với index bên dưới
CREATE TABLE "voucher_items" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id") ON DELETE CASCADE,
    "sku" VARCHAR(50) NOT NULL, 
    "quantity" INT DEFAULT 1 CHECK ("quantity" > 0), -- Đã fix lỗi tên cột trong CHECK
    "type" voucher_item_type,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE
);

CREATE TABLE "voucher_usages" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "user_voucher_id" UUID REFERENCES "user_vouchers"("id"),
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id"),
    "order_id" UUID NOT NULL,
    "applied_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "voucher_queue" (
    "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id"),
    "user_id" UUID NOT NULL REFERENCES "loyalty_accounts"("id"),
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- NHÓM 3: TỐI ƯU HÓA TRUY VẤN (INDEXES)
-- ---------------------------------------------------------

CREATE INDEX "idx_point_logs_user_id" ON "point_logs"("user_id");
CREATE INDEX "idx_loyalty_accounts_tier" ON "loyalty_accounts"("current_tier_id");
CREATE INDEX "idx_user_vouchers_user_status" ON "user_vouchers"("user_id", "status");
CREATE INDEX "idx_voucher_usages_order_id" ON "voucher_usages"("order_id");
CREATE INDEX "idx_voucher_items_lookup" ON "voucher_items" ("voucher_id") WHERE "is_deleted" IS FALSE;
CREATE INDEX IX_point_logs_expired_at ON "point_logs" ("expired_at") WHERE "expired_at" IS NOT NULL;