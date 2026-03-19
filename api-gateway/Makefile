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
HANDLERS_DIR     = internal/api/handlers

# Lấy danh sách tất cả các thư mục con trong handlers (engagement, franchise, v.v.)
# Dùng hàm của Make nên chạy mượt trên cả Linux, Mac và Windows
HANDLER_NAMES   := $(notdir $(patsubst %/,%,$(wildcard $(HANDLERS_DIR)/*/)))

# --- Files ---
GO_MAIN_REL_PATH = cmd/server/main.go
GO_MAIN_DIR      = cmd/server
GO_BIN_OUT       = bin/main.exe

# --- Regex for Protobuf ---
PROTOBUF_REGEX = \s*protobuf:"[^"]*"\s+json:"([^",]+),omitempty"
REPLACE_REGEX  = json:"$$1"

# ==============================================================================
# CROSS-PLATFORM COMMANDS (Tự động nhận diện HĐH & Terminal)
# ==============================================================================
ifeq ($(OS),Windows_NT)
    ifneq ($(MSYSTEM),)
        # 1. Đang chạy bằng Git Bash trên Windows -> Bắt buộc dùng lệnh Linux
        MKDIR_P   = mkdir -p $(GO_PB_DIR)
        RM_PB_DIR = rm -f $(GO_PB_DIR)/*/*.pb.go
        RM_BIN    = rm -f $(GO_BIN_OUT)
        RM_DOCS   = rm -rf $(SWAGGER_DOCS_DIR)
    else
        # 2. Đang chạy bằng CMD hoặc PowerShell nguyên thủy -> Dùng lệnh Windows
        SHELL := cmd.exe
        MKDIR_P   = if not exist "$(subst /,\,$(GO_PB_DIR))" mkdir "$(subst /,\,$(GO_PB_DIR))"
        RM_PB_DIR = del /q /s "$(subst /,\,$(GO_PB_DIR))\*.pb.go" 2>nul
        RM_BIN    = del /q /f "$(subst /,\,$(GO_BIN_OUT))" 2>nul
        RM_DOCS   = rmdir /s /q "$(subst /,\,$(SWAGGER_DOCS_DIR))" 2>nul
    endif
else
    # 3. Lệnh cho Linux, macOS xịn
    MKDIR_P   = mkdir -p $(GO_PB_DIR)
    RM_PB_DIR = rm -f $(GO_PB_DIR)/*/*.pb.go
    RM_BIN    = rm -f $(GO_BIN_OUT)
    RM_DOCS   = rm -rf $(SWAGGER_DOCS_DIR)
endif

# Cú pháp này quét file .proto ở thư mục gốc và các thư mục con cấp 1
PROTO_FILES := $(wildcard $(PROTO_SOURCE_DIR)/*.proto) $(wildcard $(PROTO_SOURCE_DIR)/*/*.proto)

# ==============================================================================
# COMMANDS
# ==============================================================================

.PHONY: swagger proto install-tools gen clean help build run dev test wire

help:
	@echo "PROJECT COMMANDS:"
	@echo "  make proto            : Generate gRPC code"
	@echo "  make wire             : Generate Dependency Injection code (Google Wire)"
	@echo "  make swagger          : Generate Swagger docs for ALL folders"
	@echo "  make swagger target=x : Generate Swagger docs for a specific folder (e.g., target=engagement)"
	@echo "  make gen              : Generate wire, gRPC code AND Swagger docs"
	@echo "  make build            : Build the Go API Gateway executable"
	@echo "  make run              : Build and Run the Go Gateway locally"
	@echo "  make dev              : Full flow: Gen (Proto+Swagger) -> Build -> Run"
	@echo "  make clean            : Clean generated PB files and binaries"
	@echo "  make install-tools    : Install necessary tools (swag, wire, protoc-gen-go)"

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

# --- LOGIC ĐỘNG CHO SWAGGER ---
# Tạo danh sách các target ảo: swagger-engagement, swagger-franchise, v.v.
SWAGGER_TARGETS := $(addprefix swagger-, $(HANDLER_NAMES))

# Điểm neo chính: Nếu có cờ target=... thì chạy 1 cái, không thì chạy tất cả (SWAGGER_TARGETS)
swagger: $(if $(target), swagger-$(target), $(SWAGGER_TARGETS))
	@echo "=> Swagger docs generation completed! (Check folder ./$(SWAGGER_DOCS_DIR))"

# Rule động xử lý từng thư mục (Dấu % đại diện cho tên folder)
swagger-%:
	@echo "Regenerating Swagger for [$*]..."
	@swag init -g main.go \
		--dir ./$(GO_MAIN_DIR),./$(HANDLERS_DIR)/$* \
		--instanceName $* \
		--output $(SWAGGER_DOCS_DIR)/$* \
		--parseDependency --parseInternal

gen: proto wire swagger

# ------------------------------------------------------------------------------
# 2. GO BUILD
# ------------------------------------------------------------------------------
build:
	@echo "Building Go API Gateway..."
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
	@$(RM_PB_DIR)
	@$(RM_BIN)
	@$(RM_DOCS)
	@echo "Clean done!"