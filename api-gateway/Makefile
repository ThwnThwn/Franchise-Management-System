include .env
export

BINARY_NAME=main

.PHONY: build
build:
	@go build -o ./bin/$(BINARY_NAME).exe cmd/server/main.go
 
.PHONY: run
run: build
	@./bin/$(BINARY_NAME).exe

.PHONY: test
test:
	@go test -v ./...
