# CHB CFMS Backend Project

This is the backend repository for the CHB CFMS project, utilizing a Microservices architecture with **.NET Aspire** as the Orchestrator and **Golang** as the API Gateway.

## 📋 Prerequisites

Before you begin, ensure you have the following installed on your machine:

1.  **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (or later).
2.  **[Go (Golang)](https://go.dev/dl/)** (Version 1.21+).
3.  **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (Required for running Aspire and Databases).
4.  **Make Tool**:
    - _Linux/macOS_: Pre-installed.
    - _Windows_: Install via Chocolatey (`choco install make`) or use Git Bash.

---

## 🚀 Setup Guide

### Step 1: Install .NET Aspire Workload

This is a mandatory step to run the AppHost project. Open your terminal and run:

```bash
dotnet workload install aspire
```

### Step 2: Configure Environment Variables

The project uses a single `.env` file shared between .NET services and Go Gateway.

1. Copy `.env.example` to `.env` in the root directory.
2. Update database credentials or ports if necessary.

```bash
cp .env.example .env
```

---

## ▶️ Running the Project

We use a **Makefile** to automate the build and run process.

### Option 1: Quick Start (Recommended)

To build the Golang Gateway and start the .NET Aspire system in one go, simply run:

```bash
make run
```

_This command will:_

1. _Build the Go executable (`main.exe`) from `cmd/server/main.go` into `api-gateway/bin/`._
2. _Start the .NET Aspire AppHost._
3. _Provide the Dashboard URL (e.g., http://localhost:18888)._

### Option 2: Full Development Flow

If you have modified `.proto` files and need to regenerate the gRPC code before running:

```bash
make dev
```

_This command executes: `make gen` (Generate Proto) -> `make run` (Build & Start)._

---

## 🛠 Troubleshooting

**Q: 'make' is not recognized as an internal or external command?**
A: You are likely on Windows and haven't installed `Make`. Please install it (e.g., `choco install make`) or use the manual commands below.

**Q: Error "Address already in use"?**
A: Old processes might still be running. Run these commands (Admin) to kill them:

```powershell
taskkill /F /IM dotnet.exe
taskkill /F /IM main.exe
```

---

## 📝 Manual Commands (Fallback)

If you cannot use `Make`, you can run the project manually:

1. **Build Go Gateway:**

```bash
cd api-gateway
go build -o bin/main.exe ./cmd/server/main.go
```

2. **Run Aspire:**

```bash
cd ../backend/AppHost
dotnet run
```
