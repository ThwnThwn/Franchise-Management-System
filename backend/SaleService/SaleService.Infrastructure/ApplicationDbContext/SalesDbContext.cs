using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SalesService.Domain.Commons;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.Interceptor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SalesService.Infrastructure.ApplicationDbContext;

public partial class SalesDbContext : DbContext, ISalesDbContext
{
    private readonly UpdateTimestampInterceptor _updateInterceptor;

    public SalesDbContext()
    {
    }

    public SalesDbContext(DbContextOptions<SalesDbContext> options, UpdateTimestampInterceptor updateTimestampInterceptor)
        : base(options)
    {
        _updateInterceptor = updateTimestampInterceptor;
    }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<OrderSaleItem> OrderSaleItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentEvent> PaymentEvents { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<Wallet> Wallets { get; set; }
    public virtual DbSet<WalletEvent> WalletEvents { get; set; }
    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }
    public virtual DbSet<RefundTicket> RefundTickets { get; set; }

    public DbSet<T> Set<T>() where T : class, IEntity => base.Set<T>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cart_items_pkey");

            entity.ToTable("cart_items");

            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt }, "idx_cart_items_customer_created")
                .IsDescending(false, true)
                .HasFilter("(customer_id IS NOT NULL)");

            entity.HasIndex(e => new { e.CustomerId, e.Sku }, "idx_cart_items_customer_sku")
                .IsUnique()
                .HasFilter("(customer_id IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BasePrice)
                .HasPrecision(12, 3)
                .HasColumnName("base_price");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<OrderSaleItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_sale_items_pkey");

            entity.ToTable("order_sale_items");

            entity.HasIndex(e => e.OrderId, "idx_order_sale_items_order");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");

            entity.Property(e => e.Amount)
                .HasPrecision(12, 3)
                .HasColumnName("amount");

            entity.Property(e => e.Type)
                .HasColumnName("type");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.ReferenceId)
                .HasColumnName("reference_id");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.Property(e => e.OrderId)
                .HasColumnName("order_id");

            entity.HasOne(d => d.Order)
                .WithMany(p => p.OrderSaleItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_sale_items_order");
        });
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.HasIndex(e => e.Code, "idx_orders_code").IsUnique();

            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt }, "idx_orders_customer_created")
                .IsDescending(false, true)
                .HasFilter("(customer_id IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(12, 3)
                .HasColumnName("paid_amount");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 3)
                .HasColumnName("subtotal");
            entity.Property(e => e.Tax)
                .HasPrecision(12, 3)
                .HasColumnName("tax");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 3)
                .HasColumnName("total_amount");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.OrderId, "idx_order_items_order");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 3)
                .HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_items_order");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");

            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.ProcessedOn, "idx_outbox_messages_processed_on").HasFilter("(processed_on IS NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Error).HasColumnName("error");
            entity.Property(e => e.OccurredOn).HasColumnName("occurred_on");
            entity.Property(e => e.ProcessedOn).HasColumnName("processed_on");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.Code, "idx_payments_code").IsUnique();

            entity.HasIndex(e => e.OrderId, "idx_payments_order_id");

            entity.HasIndex(e => e.Status, "idx_payments_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 3)
                .HasColumnName("amount");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValueSql("'VND'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Method).HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasColumnName("provider");
            entity.Property(e => e.ProviderPayload)
                .HasColumnType("jsonb")
                .HasColumnName("provider_payload");
            entity.Property(e => e.ProviderTxnId)
                .HasMaxLength(100)
                .HasColumnName("provider_txn_id");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_payments_order");
        });

        modelBuilder.Entity<PaymentEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_events_pkey");

            entity.ToTable("payment_events");

            entity.HasIndex(e => e.PaymentId, "idx_payment_events_payment_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.Payload)
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentEvents)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_events_payment");
        });

        modelBuilder.Entity<RefundTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refund_tickets_pkey");

            entity.ToTable("refund_tickets");

            entity.HasIndex(e => e.TicketCode, "idx_refund_tickets_code").IsUnique();

            entity.HasIndex(e => e.Status, "idx_refund_tickets_status");

            entity.HasIndex(e => e.WalletId, "idx_refund_tickets_wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ApprovalNotes)
                .HasMaxLength(500)
                .HasColumnName("approval_notes");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .HasColumnName("bank_name");
            entity.Property(e => e.CancellationReason)
                .HasMaxLength(500)
                .HasColumnName("cancellation_reason");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByInternalUserId).HasColumnName("created_by_internal_user_id");
            entity.Property(e => e.InternalNotes)
                .HasMaxLength(500)
                .HasColumnName("internal_notes");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.OriginalTransactionId).HasColumnName("original_transaction_id");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(18, 2)
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundReason).HasColumnName("refund_reason");
            entity.Property(e => e.RefundTicketMethod).HasColumnName("refund_ticket_method");
            entity.Property(e => e.RejectedAt).HasColumnName("rejected_at");
            entity.Property(e => e.RejectedBy).HasColumnName("rejected_by");
            entity.Property(e => e.RejectionNotes)
                .HasMaxLength(500)
                .HasColumnName("rejection_notes");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TicketCode)
                .HasMaxLength(50)
                .HasColumnName("ticket_code");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.OriginalTransaction).WithMany(p => p.RefundTicketOriginalTransactions)
                .HasForeignKey(d => d.OriginalTransactionId)
                .HasConstraintName("fk_refund_original_transaction");

            entity.HasOne(d => d.Transaction).WithMany(p => p.RefundTicketTransactions)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("fk_refund_transaction");

            entity.HasOne(d => d.Wallet).WithMany(p => p.RefundTickets)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("fk_refund_wallet");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallets_pkey");

            entity.ToTable("wallets");

            entity.HasIndex(e => e.Status, "idx_wallets_status");

            entity.HasIndex(e => e.UserId, "idx_wallets_user")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.RowVersion)
                .HasDefaultValueSql("'\\x5c7830303030303030303030303030303031'::bytea")
                .HasColumnName("row_version");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<WalletEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_events_pkey");

            entity.ToTable("wallet_events");

            entity.HasIndex(e => e.Status, "idx_wallet_events_status");

            entity.HasIndex(e => e.WalletTransactionId, "idx_wallet_events_transaction");

            entity.HasIndex(e => e.EventType, "idx_wallet_events_type");

            entity.HasIndex(e => e.UserId, "idx_wallet_events_user");

            entity.HasIndex(e => new { e.WalletId, e.CreatedAt }, "idx_wallet_events_wallet").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AdminNote)
                .HasMaxLength(500)
                .HasColumnName("admin_note");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EventType).HasColumnName("event_type");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessedByAdminId).HasColumnName("processed_by_admin_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.WalletTransactionId).HasColumnName("wallet_transaction_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletEvents)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wallet_events_wallet");

            entity.HasOne(d => d.WalletTransaction).WithMany(p => p.WalletEvents)
                .HasForeignKey(d => d.WalletTransactionId)
                .HasConstraintName("fk_wallet_events_wallet_transaction");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_transactions_pkey");

            entity.ToTable("wallet_transactions");

            entity.HasIndex(e => e.TransactionCode, "idx_wallet_transactions_code").IsUnique();

            entity.HasIndex(e => e.Status, "idx_wallet_transactions_status");

            entity.HasIndex(e => e.UserId, "idx_wallet_transactions_user");

            entity.HasIndex(e => new { e.WalletId, e.CreatedAt }, "idx_wallet_transactions_wallet").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BalanceAfter)
                .HasPrecision(18, 2)
                .HasColumnName("balance_after");
            entity.Property(e => e.BalanceBefore)
                .HasPrecision(18, 2)
                .HasColumnName("balance_before");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ExternalTransactionId)
                .HasMaxLength(100)
                .HasColumnName("external_transaction_id");
            entity.Property(e => e.FailedAt).HasColumnName("failed_at");
            entity.Property(e => e.FailureReason)
                .HasMaxLength(500)
                .HasColumnName("failure_reason");
            entity.Property(e => e.InitiatedBy).HasColumnName("initiated_by");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ParentTransactionId).HasColumnName("parent_transaction_id");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(50)
                .HasColumnName("transaction_code");
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Order).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_wallet_transactions_order");

            entity.HasOne(d => d.ParentTransaction).WithMany(p => p.InverseParentTransaction)
                .HasForeignKey(d => d.ParentTransactionId)
                .HasConstraintName("fk_wallet_transactions_parent");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wallet_transactions_wallet");
        });

        modelBuilder.Entity<WithdrawalRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("withdrawal_requests_pkey");

            entity.ToTable("withdrawal_requests");

            entity.HasIndex(e => e.RequestCode, "idx_withdrawal_requests_code").IsUnique();

            entity.HasIndex(e => e.CreatedAt, "idx_withdrawal_requests_created_at").IsDescending();

            entity.HasIndex(e => e.Status, "idx_withdrawal_requests_status");

            entity.HasIndex(e => e.UserId, "idx_withdrawal_requests_user");

            entity.HasIndex(e => e.WalletId, "idx_withdrawal_requests_wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AccountName)
                .HasMaxLength(255)
                .HasColumnName("account_name");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ApprovalNotes)
                .HasMaxLength(500)
                .HasColumnName("approval_notes");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.BankCode)
                .HasMaxLength(50)
                .HasColumnName("bank_code");
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .HasColumnName("bank_name");
            entity.Property(e => e.BankTransactionId)
                .HasMaxLength(100)
                .HasColumnName("bank_transaction_id");
            entity.Property(e => e.CancellationReason)
                .HasMaxLength(500)
                .HasColumnName("cancellation_reason");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EncryptedAccountNumber).HasColumnName("encrypted_account_number");
            entity.Property(e => e.FeeAmount)
                .HasPrecision(18, 2)
                .HasColumnName("fee_amount");
            entity.Property(e => e.InternalNotes)
                .HasMaxLength(500)
                .HasColumnName("internal_notes");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MaskedAccountNumber)
                .HasMaxLength(50)
                .HasColumnName("masked_account_number");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.ReceiptImagePublicId)
                .HasMaxLength(255)
                .HasColumnName("receipt_image_public_id");
            entity.Property(e => e.ReceiptImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("receipt_image_url");
            entity.Property(e => e.RejectedAt).HasColumnName("rejected_at");
            entity.Property(e => e.RejectedBy).HasColumnName("rejected_by");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.RequestCode)
                .HasMaxLength(50)
                .HasColumnName("request_code");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.WithdrawalRequests)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("fk_withdrawal_transaction");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WithdrawalRequests)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_withdrawal_wallet");
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeletedEntity).IsAssignableFrom(entity.ClrType))
            {
                // C?u h�nh Query Filter: x => !x.IsDeleted
                modelBuilder.Entity(entity.ClrType)
                    .HasQueryFilter(GenerateIsDeletedExpression(entity.ClrType));
            }
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private static LambdaExpression GenerateIsDeletedExpression(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(SoftDeletedEntity.IsDeleted));
        var constant = Expression.Constant(false);
        var compare = Expression.Equal(property, constant);

        return Expression.Lambda(compare, parameter);
    }
}