# ==============================================================================
# CONFIG & VARIABLES
# ==============================================================================
include ../.env
export

PROTO_SOURCE_DIR = ../protos

WIRE_PATH=./internal/di

GO_PB_DIR = pkg/pb

GO_MAIN_REL_PATH = cmd/server/main.go
GO_BIN_OUT = bin/main.exe

PROTOBUF_REGEX = `protobuf:.*?"\s+json:".*?"`

# ==============================================================================
# COMMANDS
# ==============================================================================

.PHONY: gen clean help build run dev test wire

help:
	@echo "PROJECT COMMANDS:"
	@echo "  make gen    : Generate gRPC code for Go from .proto files"
	@echo "  make build  : Build the Go API Gateway executable"
	@echo "  make run    : Build and Run the Go Gateway locally"
	@echo "  make dev    : Full flow: Generate Proto -> Build -> Run"
	@echo "  make wire   : Generate Dependency Injection code (Google Wire)"
	@echo "  make clean  : Clean generated PB files and binaries"

# ------------------------------------------------------------------------------
# 1. PROTOCOL BUFFERS GENERATION
# ------------------------------------------------------------------------------
gen:
	@echo "Generating code from $(PROTO_SOURCE_DIR) to $(GO_PB_DIR)..."
	@mkdir -p $(GO_PB_DIR)
	@protoc \
		--proto_path=$(PROTO_SOURCE_DIR) \
		--go_out=$(GO_PB_DIR) --go_opt=paths=source_relative \
		--go-grpc_out=$(GO_PB_DIR) --go-grpc_opt=paths=source_relative \
		$(shell find $(PROTO_SOURCE_DIR) -name "*.proto")
	@echo "Proto generation done! Check: $(GO_PB_DIR)"

# ------------------------------------------------------------------------------
# 2. GO BUILD
# ------------------------------------------------------------------------------
build:
	@echo "Building Go API Gateway..."
	@go mod tidy
	@go build -o $(GO_BIN_OUT) ./$(GO_MAIN_REL_PATH)
	@echo "Build success! Binary located at: $(GO_BIN_OUT)"

# ------------------------------------------------------------------------------
# 3. RUN APPLICATION
# ------------------------------------------------------------------------------
run: build
	@echo "🚀 Starting API Gateway..."
	@# Chạy file .exe vừa build (Đã sửa từ $(BINARY_NAME) thành $(GO_BIN_OUT))
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

wire:
	@echo "Running Google Wire..."
	@wire $(if $(filter-out $@,$(MAKECMDGOALS)),$(WIRE_PATH)/$(filter-out $@,$(MAKECMDGOALS)),$(WIRE_PATH)/...)

# ------------------------------------------------------------------------------
# CLEANUP
# ------------------------------------------------------------------------------
clean:
	@echo "Cleaning up..."
	@rm -f $(GO_PB_DIR)/*/*.pb.go
	@rm -f $(GO_BIN_OUT)
	@echo "Clean done!"