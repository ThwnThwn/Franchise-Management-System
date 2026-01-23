include ../.env
export

BINARY_NAME=main
WIRE_PATH=./internal/di


.PHONY: build
build:
	@go build -o ./bin/$(BINARY_NAME).exe cmd/server/main.go
 
.PHONY: run
run: build
	@./bin/$(BINARY_NAME).exe

.PHONY: test
test:
	@go test -v ./...

.PHONY: wire
wire:
	@wire $(if $(filter-out $@,$(MAKECMDGOALS)),$(WIRE_PATH)/$(filter-out $@,$(MAKECMDGOALS)),$(WIRE_PATH)/...)
