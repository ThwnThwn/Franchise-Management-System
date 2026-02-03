CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Bảng danh mục nguyên liệu
CREATE TABLE "ingredients" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "name" VARCHAR(255) NOT NULL,
  "base_unit" VARCHAR(50) NOT NULL,
  "category" VARCHAR(100),
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "is_deleted" BOOLEAN DEFAULT FALSE
);

-- 2. Bảng công thức (Recipe)
CREATE TABLE "recipes" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "variant_id" UUID NOT NULL, 
  "ingredient_id" UUID NOT NULL,
  "quantity" DECIMAL(12, 3) NOT NULL,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "is_deleted" BOOLEAN DEFAULT FALSE,
  -- Ràng buộc: Một sản phẩm không được trùng nguyên liệu trong công thức
  CONSTRAINT "unique_variant_ingredient" UNIQUE ("variant_id", "ingredient_id")
);

-- 3. Bảng kho của từng cửa hàng (Store Inventory)
CREATE TABLE "store_ingredients" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "store_id" UUID NOT NULL,
  "ingredient_id" UUID NOT NULL,
  "quantity" DECIMAL(12, 3) DEFAULT 0,
  "min_threshold" DECIMAL(12, 3) DEFAULT 0, -- Chuyển sang Numeric để đồng bộ với quantity
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "is_deleted" BOOLEAN DEFAULT FALSE,
  -- Ràng buộc: Mỗi cửa hàng chỉ có 1 dòng duy nhất cho mỗi loại nguyên liệu
  CONSTRAINT "unique_store_ingredient" UNIQUE ("store_id", "ingredient_id")
);

-- THIẾT LẬP FOREIGN KEYS (Cách viết tường minh)
ALTER TABLE "recipes" 
  ADD CONSTRAINT "fk_recipes_ingredient" 
  FOREIGN KEY ("ingredient_id") REFERENCES "ingredients" ("id") ON DELETE CASCADE;

ALTER TABLE "store_ingredients" 
  ADD CONSTRAINT "fk_store_ingredients_ingredient" 
  FOREIGN KEY ("ingredient_id") REFERENCES "ingredients" ("id") ON DELETE CASCADE;

-- 4. Create Outbox Messages Table
CREATE TABLE "OutboxMessages" (
    "Id" uuid NOT NULL,
    "Type" text NOT NULL,
    "Content" text NOT NULL,
    "OccurredOn" timestamp with time zone NOT NULL,
    "ProcessedOn" timestamp with time zone NULL,
    "Error" text NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_OutboxMessages" PRIMARY KEY ("Id")
);

-- 5. Bảng hàng đợi giữ nguyên liệu (Ingredient Queue)
CREATE TABLE "ingredient_queues" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "store_id" UUID NOT NULL,
  "ingredient_id" UUID NOT NULL,
  "order_id" UUID NOT NULL,
  "amount" DECIMAL(12, 3) NOT NULL,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "expires_at" TIMESTAMP WITH TIME ZONE NOT NULL,
  "is_deleted" BOOLEAN DEFAULT FALSE
);

-- Index để tìm kiếm nhanh theo Order hoặc Store/Ingredient
CREATE INDEX "idx_ingredient_queues_order_id" ON "ingredient_queues" ("order_id");
CREATE INDEX "idx_ingredient_queues_store_ingredient" ON "ingredient_queues" ("store_id", "ingredient_id");
