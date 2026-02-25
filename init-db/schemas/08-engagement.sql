CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 0. ĐỊNH NGHĨA CÁC KIỂU ENUM
-- ---------------------------------------------------------
CREATE TYPE point_action_type AS ENUM ('REDEEM', 'EARN', 'REFUND', 'ADJUST');
CREATE TYPE point_reason_code AS ENUM ('ORDER_PAYMENT', 'BIRTHDAY', 'CANCEL_ORDER', 'PROMOTION', 'MANUAL_ADJUST');
CREATE TYPE voucher_category AS ENUM ('DISCOUNT_GIFT', 'SHIPPING');
CREATE TYPE voucher_discount_type AS ENUM ('PERCENTAGE', 'FIXED_AMOUNT', 'GIFT_ITEM');
CREATE TYPE user_voucher_status AS ENUM ('AVAILABLE', 'USED', 'EXPIRED');
CREATE TYPE voucher_item_type AS ENUM ('GIFT', 'REQUIRED');

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

CREATE TABLE "loyalty_accounts" (
    "id" UUID PRIMARY KEY,
    "available_points" DECIMAL(15, 3) DEFAULT 0 CHECK ("available_points" >= 0),
    "total_spent" DECIMAL(15, 3) DEFAULT 0 CHECK ("total_spent" >= 0),
    "current_tier_id" UUID,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "fk_loyalty_tier" FOREIGN KEY ("current_tier_id") REFERENCES "membership_tiers"("id") ON DELETE SET NULL
);

CREATE TABLE "point_logs" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "user_id" UUID REFERENCES "loyalty_accounts"("id"),
    "amount" DECIMAL(15, 3) NOT NULL,
    "balance_before" DECIMAL(15, 3),
    "balance_after" DECIMAL(15, 3),
    "action_type" point_action_type NOT NULL,
    "reason_code" point_reason_code,
    "reference_id" UUID,
    "description" TEXT,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- NHÓM 2: QUẢN LÝ VOUCHER & KHUYẾN MÃI
-- ---------------------------------------------------------

CREATE TABLE "vouchers" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "code" VARCHAR(50) NOT NULL,
    "category" voucher_category NOT NULL,
    "type" voucher_discount_type,
    "discount_value" DECIMAL(15, 3) CHECK ("discount_value" >= 0),
    "min_order_value" DECIMAL(15, 3) DEFAULT 0 CHECK ("min_order_value" >= 0),
    "point_cost" DECIMAL(15, 3) DEFAULT 0 CHECK ("point_cost" >= 0),
    "init_quantity" INT DEFAULT 0 CHECK ("init_quantity" >= 0),
    "current_quantity" INT DEFAULT 0 CHECK ("current_quantity" >= 0),
    "max_usage_per_user" INT NOT NULL DEFAULT 1 CHECK ("max_usage_per_user" >= 0), 
    "is_active" BOOLEAN DEFAULT TRUE,
    "end_date" TIMESTAMP WITH TIME ZONE,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "check_quantity_logic" CHECK ("current_quantity" <= "init_quantity")
);

CREATE UNIQUE INDEX "uq_voucher_code_active" ON "vouchers" ("code") WHERE ("is_deleted" IS FALSE);

CREATE TABLE "user_vouchers" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "user_id" UUID NOT NULL REFERENCES "loyalty_accounts"("id"),
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id"),
    "status" user_voucher_status DEFAULT 'AVAILABLE',
    "acquired_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "expires_at" TIMESTAMP WITH TIME ZONE,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE
);

-- Đã đổi tên bảng thành voucher_items để đồng bộ với index bên dưới
CREATE TABLE "voucher_items" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id") ON DELETE CASCADE,
    "variant_id" UUID NOT NULL, 
    "quantity" INT DEFAULT 1 CHECK ("quantity" > 0), -- Đã fix lỗi tên cột trong CHECK
    "type" voucher_item_type DEFAULT 'REQUIRED',
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "is_deleted" BOOLEAN DEFAULT FALSE
);

CREATE TABLE "voucher_usages" (
    "id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "user_voucher_id" UUID REFERENCES "user_vouchers"("id"),
    "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id"),
    "order_id" UUID NOT NULL,
    "store_id" UUID NOT NULL,
    "discount_amount" DECIMAL(15, 3) DEFAULT 0,
    "applied_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "voucher_queue" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "voucher_id" UUID NOT NULL REFERENCES "vouchers"("id"),
  "order_id" UUID NOT NULL,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "expires_at" TIMESTAMP WITH TIME ZONE NOT NULL
);

-- NHÓM 3: TỐI ƯU HÓA TRUY VẤN (INDEXES)
-- ---------------------------------------------------------
CREATE INDEX "idx_point_logs_user_id" ON "point_logs"("user_id");
CREATE INDEX "idx_loyalty_accounts_tier" ON "loyalty_accounts"("current_tier_id");
CREATE INDEX "idx_user_vouchers_user_status" ON "user_vouchers"("user_id", "status");
CREATE INDEX "idx_voucher_usages_order_id" ON "voucher_usages"("order_id");
CREATE INDEX "idx_voucher_usages_store_id" ON "voucher_usages"("store_id");
CREATE INDEX "idx_voucher_items_lookup" ON "voucher_items" ("voucher_id") WHERE "is_deleted" IS FALSE;