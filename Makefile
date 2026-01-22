PROTO_SOURCE_DIR = protos

GO_PB_DIR = api-gateway/pkg/pb

# ==============================================================================
# COMMANDS
# ==============================================================================

.PHONY: gen clean help

help:
	@echo "PROJECT COMMANDS:"
	@echo "  make gen    : Generate all code gRPC for Go"
	@echo "  make clean  : Delele all old PB file in Go"

# Generate Code
gen:
	@echo "Generating code from $(PROTO_SOURCE_DIR) to $(GO_PB_DIR)..."
	@mkdir -p $(GO_PB_DIR)
	@protoc \
		--proto_path=$(PROTO_SOURCE_DIR) \
		--go_out=$(GO_PB_DIR) --go_opt=paths=source_relative \
		--go-grpc_out=$(GO_PB_DIR) --go-grpc_opt=paths=source_relative \
		$(shell find $(PROTO_SOURCE_DIR) -name "*.proto")
	@echo "Done! Check your code in $(GO_PB_DIR)"

# Clean up
clean:
	@echo "Cleaning all the old .pb.go ..."
	@rm -f $(GO_PB_DIR)/*/*.pb.go
	@echo "Done!"