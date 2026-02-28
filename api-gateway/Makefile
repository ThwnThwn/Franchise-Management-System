# ==============================================================================
# CONFIG & VARIABLES
# ==============================================================================
include ../.env
export

# --- Directories ---
PROTO_SOURCE_DIR = ../protos
WIRE_PATH        = ./internal/di
GO_PB_DIR        = pkg/pb
SWAGGER_DOCS_DIR = docs

# --- Files ---
GO_MAIN_REL_PATH = cmd/server/main.go
GO_BIN_OUT       = bin/main.exe

# --- Regex for Protobuf ---
PROTOBUF_REGEX = \s*protobuf:"[^"]*"\s+json:"([^",]+),omitempty"
REPLACE_REGEX  = json:"$1"

# ==============================================================================
# CROSS-PLATFORM COMMANDS (Tự động nhận diện HĐH)
# ==============================================================================
ifeq ($(OS),Windows_NT)
    # Lệnh cho Windows (PowerShell, CMD)
    MKDIR_P   = if not exist "$(subst /,\,$(GO_PB_DIR))" mkdir "$(subst /,\,$(GO_PB_DIR))"
    RM_PB_DIR = del /q /s "$(subst /,\,$(GO_PB_DIR))\*.pb.go" 2>nul
    RM_BIN    = del /q /f "$(subst /,\,$(GO_BIN_OUT))" 2>nul
    RM_DOCS   = rmdir /s /q "$(subst /,\,$(SWAGGER_DOCS_DIR))" 2>nul
else
    # Lệnh cho Linux, macOS, hoặc Git Bash
    MKDIR_P   = mkdir -p $(GO_PB_DIR)
    RM_PB_DIR = rm -f $(GO_PB_DIR)/*/*.pb.go
    RM_BIN    = rm -f $(GO_BIN_OUT)
    RM_DOCS   = rm -rf $(SWAGGER_DOCS_DIR)
endif

# Dùng hàm của Make để tự tìm file .proto, không phụ thuộc vào shell (find hay dir)
# Cú pháp này quét file .proto ở thư mục gốc và các thư mục con cấp 1
PROTO_FILES := $(wildcard $(PROTO_SOURCE_DIR)/*.proto) $(wildcard $(PROTO_SOURCE_DIR)/*/*.proto)

# ==============================================================================
# COMMANDS
# ==============================================================================

.PHONY: swagger proto install-tools gen clean help build run dev test wire

help:
	@echo "PROJECT COMMANDS:"
	@echo "  make proto         : Generate gRPC code"
	@echo "  make wire          : Generate Dependency Injection code (Google Wire)"
	@echo "  make swagger       : Generate Swagger documentation"
	@echo "  make gen           : Generate wire, gRPC code AND Swagger docs"
	@echo "  make build         : Build the Go API Gateway executable"
	@echo "  make run           : Build and Run the Go Gateway locally"
	@echo "  make dev           : Full flow: Gen (Proto+Swagger) -> Build -> Run"
	@echo "  make clean         : Clean generated PB files and binaries"
	@echo "  make install-tools : Install necessary tools (swag, wire, protoc-gen-go)"

# ------------------------------------------------------------------------------
# 1. CODE GENERATION (PROTO + WIRE + SWAGGER)
# ------------------------------------------------------------------------------
proto:
	@echo "Generating code from $(PROTO_SOURCE_DIR) to $(GO_PB_DIR)..."
	@$(MKDIR_P)
	@protoc \
		--proto_path=$(PROTO_SOURCE_DIR) \
		--go_out=$(GO_PB_DIR) --go_opt=paths=source_relative \
		--go-grpc_out=$(GO_PB_DIR) --go-grpc_opt=paths=source_relative \
		$(PROTO_FILES)
	@echo "Proto generation process finished! Check: $(GO_PB_DIR)"

wire:
	@echo "Running Google Wire..."
	@wire $(if $(filter-out $@,$(MAKECMDGOALS)),$(WIRE_PATH)/$(filter-out $@,$(MAKECMDGOALS)),$(WIRE_PATH)/...)
	@echo "Google Wire generation process finished!"

swagger:
	@echo "Regenerating Swagger documentation to $(SWAGGER_DOCS_DIR)..."
	@swag init -g $(GO_MAIN_REL_PATH) \
		--output $(SWAGGER_DOCS_DIR) \
		--parseDependency --parseInternal
	@echo "Swagger docs updated! (Check ./$(SWAGGER_DOCS_DIR) folder)"

gen: proto wire swagger

# ------------------------------------------------------------------------------
# 2. GO BUILD
# ------------------------------------------------------------------------------
build:
	@echo "Building Go API Gateway..."
# 	@go mod tidy
	@go build -o $(GO_BIN_OUT) ./$(GO_MAIN_REL_PATH)
	@echo "Build success! Binary located at: $(GO_BIN_OUT)"

# ------------------------------------------------------------------------------
# 3. RUN APPLICATION
# ------------------------------------------------------------------------------
run: build
	@echo "🚀 Starting API Gateway..."
	@./$(GO_BIN_OUT)

# ------------------------------------------------------------------------------
# 4. DEVELOPMENT (FULL FLOW)
# ------------------------------------------------------------------------------
dev: gen run

# ------------------------------------------------------------------------------
# 5. TESTING & TOOLS
# ------------------------------------------------------------------------------
test:
	@go test -v ./...

install-tools:
	@echo "Installing tools..."
	@go install github.com/swaggo/swag/cmd/swag@latest
	@go install github.com/google/wire/cmd/wire@latest
	@go install google.golang.org/protobuf/cmd/protoc-gen-go@latest
	@go install google.golang.org/grpc/cmd/protoc-gen-go-grpc@latest
	@echo "All tools installed."

# ------------------------------------------------------------------------------
# CLEANUP
# ------------------------------------------------------------------------------
clean:
	@echo "Cleaning up..."
	@rm -f $(GO_PB_DIR)/*/*.pb.go
	@rm -f $(GO_BIN_OUT)
	@rm -rf $(SWAGGER_DOCS_DIR)
	@echo "Clean done!"