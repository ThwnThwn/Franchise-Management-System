#!/bin/bash

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
    # Force psql connect to DB (-d) to run file (-f)
    psql -U "$POSTGRES_USER" -d "$db_name" -f "$file_path"
}

# Core Services
create_db_safe "identity_db"
create_db_safe "product_db"
create_db_safe "sales_db"
create_db_safe "inventory_db"
create_db_safe "franchise_db"
create_db_safe "payment_db"

run_sql_script "identity_db" "/docker-entrypoint-initdb.d/schemas/02-identity.sql"
run_sql_script "product_db" "/docker-entrypoint-initdb.d/schemas/03-product.sql"
run_sql_script "sales_db" "/docker-entrypoint-initdb.d/schemas/04-sales.sql"
run_sql_script "inventory_db" "/docker-entrypoint-initdb.d/schemas/05-inventory.sql"
run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/schemas/06-franchise.sql"
run_sql_script "payment_db" "/docker-entrypoint-initdb.d/schemas/07-payment.sql"

# Seed data
run_sql_script "identity_db" "/docker-entrypoint-initdb.d/seeding/01-init-seeding-identity.sql"
run_sql_script "product_db" "/docker-entrypoint-initdb.d/seeding/02-seeding-product.sql"
run_sql_script "inventory_db" "/docker-entrypoint-initdb.d/seeding/05-seeding-inventory.sql"
run_sql_script "franchise_db" "/docker-entrypoint-initdb.d/seeding/06-seeding-franchise.sql"

echo "Database initialization process finished!"