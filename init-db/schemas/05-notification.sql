-- ================================================
-- NotificationService Database Schema
-- Generated from EF Core Migrations
-- ================================================

-- Create NotificationLogs table
CREATE TABLE IF NOT EXISTS "NotificationLogs" (
    "Id" uuid NOT NULL,
    "Type" integer NOT NULL,
    "Recipient" text NOT NULL,
    "Subject" text NOT NULL,
    "Metadata" jsonb NULL,
    "Status" integer NOT NULL,
    "SentAt" timestamp with time zone NULL,
    "ErrorMessage" text NULL,
    "RetryCount" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ReferenceId" text NULL,
    CONSTRAINT "PK_NotificationLogs" PRIMARY KEY ("Id")
);

-- Create index on ReferenceId
CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_ReferenceId" 
    ON "NotificationLogs" ("ReferenceId");

-- Create index on Type
CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_Type" 
    ON "NotificationLogs" ("Type");

-- Create index on Status
CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_Status" 
    ON "NotificationLogs" ("Status");

-- Create index on CreatedAt
CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_CreatedAt" 
    ON "NotificationLogs" ("CreatedAt");

-- Create Migrations History table (for EF Core tracking)
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert migration record
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260129085714_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
