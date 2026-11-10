CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ── warehouses ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS warehouses (
  id         UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
  code       VARCHAR(50)  NOT NULL,
  name       VARCHAR(255) NOT NULL,
  address    TEXT         NOT NULL,
  store_id   UUID         NOT NULL,
  status     INT          NOT NULL DEFAULT 1,
  is_deleted BOOLEAN      NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMPTZ  NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX ux_warehouses_code
  ON warehouses(code) WHERE is_deleted = FALSE;

-- ── warehouse_items ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS warehouse_items (
  id            UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
  warehouse_id  UUID           NOT NULL,
  ingredient_id UUID           NOT NULL,
  unit          VARCHAR(50)    NOT NULL,
  quantity      DECIMAL(12,3)  NOT NULL DEFAULT 0 CHECK (quantity >= 0),
  base_price    DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (base_price >= 0),
  expiry_date   DATE,
  is_deleted    BOOLEAN        NOT NULL DEFAULT FALSE,
  created_at    TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (warehouse_id) REFERENCES warehouses(id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ux_warehouse_items_warehouse_ingredient
  ON warehouse_items(warehouse_id, ingredient_id) WHERE is_deleted = FALSE;

-- ── warehouse_transfers ───────────────────────────────────────
CREATE TABLE IF NOT EXISTS warehouse_transfers (
  id                UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
  transfer_code     VARCHAR(50) NOT NULL,
  from_warehouse_id UUID        NOT NULL,
  to_warehouse_id   UUID        NOT NULL,
  status            INT         NOT NULL DEFAULT 1,
  reason            TEXT,
  note              TEXT,
  created_by        UUID        NOT NULL,   
  approved_by       UUID,
  approved_at       TIMESTAMPTZ,
  is_deleted        BOOLEAN     NOT NULL DEFAULT FALSE,
  created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (from_warehouse_id) REFERENCES warehouses(id) ON DELETE RESTRICT,
  FOREIGN KEY (to_warehouse_id)   REFERENCES warehouses(id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ux_warehouse_transfers_code
  ON warehouse_transfers(transfer_code) WHERE is_deleted = FALSE;

-- ── stock_transactions ────────────────────────────────────────
CREATE TABLE IF NOT EXISTS stock_transactions (
  id                    UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
  transaction_code      VARCHAR(50)    NOT NULL,
  warehouse_id          UUID           NOT NULL,
  warehouse_item_id     UUID           NOT NULL,
  type                  INT            NOT NULL,
  quantity              DECIMAL(12,3)  NOT NULL CHECK (quantity > 0),
  unit_price            DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (unit_price >= 0),
  total_price           DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (total_price >= 0),
  supplier_id           UUID,
  reference_code        VARCHAR(100),
  warehouse_transfer_id UUID,
  note                  TEXT,
  status                INT            NOT NULL DEFAULT 1,
  created_by            UUID           NOT NULL,   
  is_deleted            BOOLEAN        NOT NULL DEFAULT FALSE,
  created_at            TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at            TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (warehouse_id)          REFERENCES warehouses(id)       ON DELETE RESTRICT,
  FOREIGN KEY (warehouse_item_id)     REFERENCES warehouse_items(id)  ON DELETE RESTRICT,
  FOREIGN KEY (warehouse_transfer_id) REFERENCES warehouse_transfers(id) ON DELETE SET NULL
);

CREATE UNIQUE INDEX ux_stock_transactions_code
  ON stock_transactions(transaction_code) WHERE is_deleted = FALSE;

CREATE INDEX idx_stock_transactions_warehouse
  ON stock_transactions(warehouse_id);

CREATE INDEX idx_stock_transactions_warehouse_item
  ON stock_transactions(warehouse_item_id);

CREATE INDEX idx_stock_transactions_supplier
  ON stock_transactions(supplier_id) WHERE supplier_id IS NOT NULL;

CREATE INDEX idx_stock_transactions_reference_code
  ON stock_transactions(reference_code) WHERE reference_code IS NOT NULL;

CREATE INDEX idx_stock_transactions_transfer
  ON stock_transactions(warehouse_transfer_id) WHERE warehouse_transfer_id IS NOT NULL;


-- ── warehouse_transfer_items ──────────────────────────────────
  CREATE TABLE IF NOT EXISTS warehouse_transfer_items (
  id                    UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
  warehouse_transfer_id UUID           NOT NULL,
  warehouse_item_id     UUID           NOT NULL,
  ingredient_id         UUID           NOT NULL,
  unit_price            DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (unit_price >= 0),
  quantity              DECIMAL(12,3)  NOT NULL CHECK (quantity > 0),
  created_at            TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at            TIMESTAMPTZ    NOT NULL DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (warehouse_transfer_id)
    REFERENCES warehouse_transfers(id) ON DELETE CASCADE,
  FOREIGN KEY (warehouse_item_id)
    REFERENCES warehouse_items(id)     ON DELETE RESTRICT
);
 
CREATE INDEX idx_warehouse_transfer_items_transfer
  ON warehouse_transfer_items(warehouse_transfer_id);
 