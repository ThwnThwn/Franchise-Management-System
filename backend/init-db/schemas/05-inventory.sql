CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Bảng danh mục nguyên liệu (Mới thêm)
CREATE TABLE "ingredient_categories" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "name" VARCHAR(255) NOT NULL,
  "description" TEXT,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "is_deleted" BOOLEAN DEFAULT FALSE
);

-- 2. Bảng nguyên liệu
CREATE TABLE "ingredients" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "category_id" UUID, -- Khóa ngoại liên kết tới bảng danh mục
  "name" VARCHAR(255) NOT NULL,
  "base_unit" VARCHAR(50) NOT NULL,
  "cost" DECIMAL(18, 2) NOT NULL DEFAULT 0,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "is_deleted" BOOLEAN DEFAULT FALSE,
  
  CONSTRAINT "fk_ingredients_category" 
    FOREIGN KEY ("category_id") 
    REFERENCES "ingredient_categories" ("id") 
    ON DELETE SET NULL
);

-- 3. Bảng công thức (Recipe)
CREATE TABLE "recipes" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "variant_id" UUID NOT NULL, 
  "ingredient_id" UUID NOT NULL,
  "quantity" DECIMAL(12, 3) NOT NULL,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

  
  -- Ràng buộc: Một sản phẩm không được trùng nguyên liệu trong công thức
  CONSTRAINT "unique_variant_ingredient" UNIQUE ("variant_id", "ingredient_id"),
  
  CONSTRAINT "fk_recipes_ingredient" 
    FOREIGN KEY ("ingredient_id") 
    REFERENCES "ingredients" ("id") 
    ON DELETE CASCADE
);

-- 4. Bảng kho của từng cửa hàng (Store Inventory)
CREATE TABLE "store_ingredients" (
  "id" UUID NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
  "store_id" UUID NOT NULL,
  "ingredient_id" UUID NOT NULL,
  "quantity" DECIMAL(12, 3) DEFAULT 0,
  "min_threshold" DECIMAL(12, 3) DEFAULT 0,
  "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  "is_deleted" BOOLEAN DEFAULT FALSE,
  
  -- Ràng buộc: Mỗi cửa hàng chỉ có 1 dòng duy nhất cho mỗi loại nguyên liệu
  CONSTRAINT "unique_store_ingredient" UNIQUE ("store_id", "ingredient_id"),
  
  CONSTRAINT "fk_store_ingredients_ingredient" 
    FOREIGN KEY ("ingredient_id") 
    REFERENCES "ingredients" ("id") 
    ON DELETE CASCADE
);

-- 5. Bảng Outbox Messages (Đã chuyển về snake_case để đồng bộ)
CREATE TABLE "outbox_messages" (
    "id" UUID NOT NULL PRIMARY KEY,
    "type" TEXT NOT NULL,
    "content" TEXT NOT NULL,
    "occurred_on" TIMESTAMP WITH TIME ZONE NOT NULL,
    "processed_on" TIMESTAMP WITH TIME ZONE NULL,
    "error" TEXT NULL,
    "created_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 6. Bảng hàng đợi giữ nguyên liệu (Ingredient Queue) - FROM HEAD
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

-- 7. Tối ưu hóa truy vấn (Indexes) - FROM MAIN
CREATE INDEX "idx_ingredients_category" ON "ingredients"("category_id");
CREATE INDEX "idx_recipes_variant" ON "recipes"("variant_id");
CREATE INDEX "idx_store_ingredients_store" ON "store_ingredients"("store_id");
