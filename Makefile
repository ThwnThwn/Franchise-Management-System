# ==============================================================================
# VARIABLES
# ==============================================================================
PROTO_SOURCE_DIR = protos
GO_PB_DIR = api-gateway/pkg/pb

# Paths for Go Build
GO_MODULE_DIR = api-gateway
GO_MAIN_REL_PATH = cmd/server/main.go
GO_BIN_OUT = $(GO_MODULE_DIR)/bin/main.exe

# Paths for .NET Aspire
APPHOST_DIR = backend/AppHost

# ==============================================================================
# COMMANDS
# ==============================================================================

.PHONY: gen clean help build run dev

help:
	@echo "PROJECT COMMANDS:"
	@echo "  make gen    : Generate gRPC code for Go from .proto files"
	@echo "  make build  : Build the Go API Gateway executable"
	@echo "  make run    : Build Go Gateway then Start .NET Aspire AppHost"
	@echo "  make dev    : Generate Proto -> Build Go -> Run AppHost (Full flow)"
	@echo "  make clean  : Clean generated PB files and Go binaries"

# ------------------------------------------------------------------------------
# 1. PROTOCOL BUFFERS
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
# 	@cd $(GO_MODULE_DIR) && go mod tidy
	@cd $(GO_MODULE_DIR) && go build -o bin/main.exe ./$(GO_MAIN_REL_PATH)
	@echo "Build success! Binary located at: $(GO_BIN_OUT)"

# ------------------------------------------------------------------------------
# 3. RUN APPLICATION (ASPIRE)
# ------------------------------------------------------------------------------
# 'run' depends on 'build', so it always builds Go before running Aspire
run: build
	@echo "🚀 Starting .NET Aspire Orchestrator..."
	@cd $(APPHOST_DIR) && dotnet run

# ------------------------------------------------------------------------------
# 4. DEVELOPMENT (FULL FLOW)
# ------------------------------------------------------------------------------
dev: gen run

# ------------------------------------------------------------------------------
# CLEANUP
# ------------------------------------------------------------------------------
clean:
	@echo "Cleaning up..."
	@rm -f $(GO_PB_DIR)/*/*.pb.go
	@rm -f $(GO_BIN_OUT)
	@echo "Clean done!"