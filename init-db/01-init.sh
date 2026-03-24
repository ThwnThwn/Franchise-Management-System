#!/bin/bash
set -e

GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo "Starting database creation process..."

create_db_safe() {
    local db_name=$1
    
    # 1. Check db exists
    exists=$(psql -U "$POSTGRES_USER" -tAc "SELECT 1 FROM pg_database WHERE datname='$db_name'" 2>/dev/null)
    
    if [ "$exists" = "1" ]; then
        echo -e "${GREEN}SKIP:${NC} Database '$db_name' already exists."
    else
        # 2. Create new if not exists
        if psql -U "$POSTGRES_USER" -c "CREATE DATABASE $db_name" 2>/dev/null; then
            echo -e "${GREEN}SUCCESS:${NC} Database '$db_name' created successfully."
        else
            # 3. If err, then echo
            echo -e "${RED}ERROR:${NC} Failed to create database '$db_name'. Please check logs."
        fi
    fi
}

run_sql_script() {
    local db_name=$1
    local file_path=$2
    
    echo "--- Executing $file_path into $db_name ---"
    
    if [ ! -f "$file_path" ]; then
        echo -e "${RED}ERROR:${NC} File $file_path NOT FOUND!"
        exit 1
    fi

    if psql -U "$POSTGRES_USER" -d "$db_name" -f "$file_path"; then
        echo -e "${GREEN}SUCCESS:${NC} Applied $file_path to $db_name"
    else
        echo -e "${RED}CRITICAL ERROR:${NC} Failed to apply $file_path to $db_name"
        exit 1 
    fi
}

# Core Services
create_db_safe "identity_db"
create_db_safe "product_db"
create_db_safe "sales_db"
# create_db_safe "payment_db"
create_db_safe "notification_db"
create_db_safe "inventory_db"
create_db_safe "franchise_db"
create_db_safe "engagement_db"
create_db_safe "supplier_db"


run_sql_script "identity_db" "/docker-entrypoint-initdb.d/schemas/02-identity.sql"
run_sql_script "product_db" "/docker-entrypoint-initdb.d/schemas/03-product.sql"
run_sql_script "sales_db" "/docker-entrypoint-initdb.d/schemas/04-sales.sql"
run_sql_script "notification_db" "/docker-entrypoint-initdb.d/schemas/05-notification.sql"
run_sql_script "inventory_db" "/docker-entrypoint-initdb.d/schemas/05-inventory.sql"
run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/schemas/06-franchise.sql"
# run_sql_script "payment_db" "/docker-entrypoint-initdb.d/schemas/07-payment.sql"
run_sql_script "engagement_db" "/docker-entrypoint-initdb.d/schemas/08-engagement.sql"
run_sql_script "supplier_db" "/docker-entrypoint-initdb.d/schemas/09-supplier.sql"


# Seed data
# run_sql_script "identity_db" "/docker-entrypoint-initdb.d/seeding/01-init-seeding-identity.sql"
# run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/seeding/06-seeding-franchise.sql"
# run_sql_script "product_db" "/docker-entrypoint-initdb.d/seeding/02-seeding-product.sql"
# run_sql_script "inventory_db" "/docker-entrypoint-initdb.d/seeding/05-seeding-inventory.sql"
# run_sql_script "inventory_db" "/docker-entrypoint-initdb.d/seeding/999-seeding-inventory-test.sql"
# run_sql_script "product_db" "/docker-entrypoint-initdb.d/seeding/998-seeding-product-test.sql"

# seed test release
run_sql_script "inventory_db" "/docker-entrypoint-initdb.d/seeding/realease-init-inventory.sql"
run_sql_script "product_db" "/docker-entrypoint-initdb.d/seeding/realease-init-product.sql"
run_sql_script "identity_db" "/docker-entrypoint-initdb.d/seeding/realease-init-identity.sql"
run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/seeding/realease-init-franchise.sql"
# run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/seeding/09-seeding-swap-test.sql"
# run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/seeding/10-seeding-autofill-and-cho-ca.sql"
# run_sql_script "identity_db" "/docker-entrypoint-initdb.d/seeding/11-seeding-greedy-test-identity.sql"
# run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/seeding/11-seeding-greedy-test.sql"
run_sql_script "engagement_db" "/docker-entrypoint-initdb.d/seeding/realease-init-engagement.sql"
run_sql_script "supplier_db" "/docker-entrypoint-initdb.d/seeding/08-seeding-supplier.sql"
run_sql_script "sales_db" "/docker-entrypoint-initdb.d/seeding/realease-init-sales.sql"

echo "Database initialization process finished!"