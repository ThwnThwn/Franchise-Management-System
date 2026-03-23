using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using FranchiseService.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FranchiseService.Infrastructure.ApplicationDbContext;

public partial class FranchiseDbContext : DbContext, IFranchiseDbContext
{
    private readonly UpdateTimestampInterceptor? _updateInterceptor;

    public FranchiseDbContext()
    {
    }

    public FranchiseDbContext(DbContextOptions<FranchiseDbContext> options, UpdateTimestampInterceptor updateTimestampInterceptor)
        : base(options)
    {
        _updateInterceptor = updateTimestampInterceptor;
    }

    // ... (DbSets)
    public virtual DbSet<Franchisee> Franchisees { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<ShiftAssignment> ShiftAssignments { get; set; }

    public virtual DbSet<ShiftRule> ShiftRules { get; set; }

    public virtual DbSet<ShiftTemplate> ShiftTemplates { get; set; }

    public virtual DbSet<StaffAvailability> StaffAvailabilities { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreHour> StoreHours { get; set; }

    public virtual DbSet<TimeLog> TimeLogs { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    /// <summary>
    /// Don dang ky nhan ca tu Cho Ca (Shift Pool)
    /// </summary>
    public virtual DbSet<ShiftApplication> ShiftApplications { get; set; }

    /// <summary>
    /// Yeu cau doi ca giua 2 nhan vien
    /// </summary>
    public virtual DbSet<ShiftSwapRequest> ShiftSwapRequests { get; set; }

    public virtual DbSet<FranchiseApplication> FranchiseApplications { get; set; }

    public virtual DbSet<FranchiseContract> FranchiseContracts { get; set; }

    public new DbSet<T> Set<T>() where T : class, IEntity => base.Set<T>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_updateInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_updateInterceptor);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Franchisee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("franchisees_pkey");

            entity.ToTable("franchisees");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BankAccountHolder)
                .HasMaxLength(100)
                .HasColumnName("bank_account_holder");
            entity.Property(e => e.BankAccountNumber)
                .HasMaxLength(50)
                .HasColumnName("bank_account_number");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .HasColumnName("bank_name");
            entity.Property(e => e.CommissionRate)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("commission_rate");
            entity.Property(e => e.ContractEndDate).HasColumnName("contract_end_date");
            entity.Property(e => e.ContractStartDate).HasColumnName("contract_start_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RepresentativeName)
                .HasMaxLength(100)
                .HasColumnName("representative_name");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .HasColumnName("tax_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shifts_pkey");

            entity.ToTable("shifts");

            entity.HasIndex(e => new { e.StoreId, e.ShiftDate }, "idx_shifts_store_date");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            
            entity.Property(e => e.BreakDurationMinutes)
                .HasDefaultValue(0)
                .HasColumnName("break_duration_minutes");

            entity.Property(e => e.LockVersion)
                .HasDefaultValue(1)
                .HasColumnName("lock_version");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.RequiredStaffCount)
                .HasDefaultValue(1)
                .HasColumnName("required_staff_count");
            entity.Property(e => e.ShiftDate).HasColumnName("shift_date");
            entity.Property(e => e.ShiftTemplateId).HasColumnName("shift_template_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasDefaultValue((int)ShiftStatus.Draft)
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.Shifts)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_shifts_store");
        });

        modelBuilder.Entity<ShiftAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shift_assignments_pkey");

            entity.ToTable("shift_assignments");

            entity.HasIndex(e => e.UserId, "idx_assignments_user");

            entity.HasIndex(e => new { e.ShiftId, e.UserId }, "uq_assignment_user_shift").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.HourlyWage)
                .HasPrecision(10, 2)
                .HasColumnName("hourly_wage");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.Status)
                .HasDefaultValue((int)ShiftAssignmentStatus.Assigned)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            // Theo dõi ghi đè cảnh báo validation
            entity.Property(e => e.IsOverridden)
                .HasDefaultValue(false)
                .HasColumnName("is_overridden");
            entity.Property(e => e.OverrideReason).HasColumnName("override_reason");
            entity.Property(e => e.OverriddenBy).HasColumnName("overridden_by");

            entity.HasOne(d => d.Shift).WithMany(p => p.ShiftAssignments)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assignments_shift");
        });

        modelBuilder.Entity<ShiftRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shift_rules_pkey");

            entity.ToTable("shift_rules");

            entity.HasIndex(e => e.StoreId, "idx_rules_store_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AllowShiftSwap)
                .HasDefaultValue(true)
                .HasColumnName("allow_shift_swap");
            entity.Property(e => e.AllowShuttle)
                .HasDefaultValue(false)
                .HasColumnName("allow_shuttle");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MaxHoursPerWeek)
                .HasDefaultValue(48)
                .HasColumnName("max_hours_per_week");
            entity.Property(e => e.MinHoursBetweenShifts)
                .HasDefaultValue(8)
                .HasColumnName("min_hours_between_shifts");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            // Quy tắc OT và châm chước
            entity.Property(e => e.GracePeriodMinutes)
                .HasDefaultValue(5)
                .HasColumnName("grace_period_minutes");
            entity.Property(e => e.DailyOtThresholdHours)
                .HasDefaultValue(8)
                .HasColumnName("daily_ot_threshold_hours");
            entity.Property(e => e.WeeklyOtThresholdHours)
                .HasDefaultValue(40)
                .HasColumnName("weekly_ot_threshold_hours");
        });

        modelBuilder.Entity<ShiftTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shift_templates_pkey");

            entity.ToTable("shift_templates");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BreakDurationMinutes)
                .HasDefaultValue(0)
                .HasColumnName("break_duration_minutes");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<StaffAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("staff_availabilities_pkey");

            entity.ToTable("staff_availabilities");

            entity.HasIndex(e => new { e.UserId, e.IsAvailable, e.SpecificDate, e.DayOfWeek }, "idx_staff_availability_query").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("is_available");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsRecurring)
                .HasDefaultValue(false)
                .HasColumnName("is_recurring");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
            entity.Property(e => e.SpecificDate).HasColumnName("specific_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stores_pkey");

            entity.ToTable("stores");

            entity.HasIndex(e => e.Code, "idx_stores_code").IsUnique();

            entity.HasIndex(e => e.FranchiseeId, "idx_stores_franchisee");

            entity.HasIndex(e => new { e.Latitude, e.Longitude }, "idx_stores_location");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FranchiseeId).HasColumnName("franchisee_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8)
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasDefaultValue((int)StoreStatus.Open)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasDefaultValue((int)StoreType.CompanyOwned)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Franchisee).WithMany(p => p.Stores)
                .HasForeignKey(d => d.FranchiseeId)
                .HasConstraintName("fk_stores_franchisee");
        });

        modelBuilder.Entity<StoreHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("store_hours_pkey");

            entity.ToTable("store_hours");

            entity.HasIndex(e => new { e.StoreId, e.DayOfWeek }, "idx_hours_store_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CloseTime).HasColumnName("close_time");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.IsClosed)
                .HasDefaultValue(false)
                .HasColumnName("is_closed");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.OpenTime).HasColumnName("open_time");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreHours)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("fk_hours_store");
        });

        modelBuilder.Entity<TimeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("time_logs_pkey");

            entity.ToTable("time_logs");

            entity.HasIndex(e => e.ShiftAssignmentId, "idx_timelogs_assignment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CheckInAt).HasColumnName("check_in_at");
            entity.Property(e => e.CheckInImageUrl)
                .HasMaxLength(500)
                .HasColumnName("check_in_image_url");
            entity.Property(e => e.CheckInLat)
                .HasPrecision(10, 8)
                .HasColumnName("check_in_lat");
            entity.Property(e => e.CheckInLong)
                .HasPrecision(11, 8)
                .HasColumnName("check_in_long");
            entity.Property(e => e.CheckOutAt).HasColumnName("check_out_at");
            entity.Property(e => e.CheckOutImageUrl)
                .HasMaxLength(500)
                .HasColumnName("check_out_image_url");
            entity.Property(e => e.CheckOutLat)
                .HasPrecision(10, 8)
                .HasColumnName("check_out_lat");
            entity.Property(e => e.CheckOutLong)
                .HasPrecision(11, 8)
                .HasColumnName("check_out_long");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ManagerNote).HasColumnName("manager_note");
            entity.Property(e => e.ShiftAssignmentId).HasColumnName("shift_assignment_id");
            entity.Property(e => e.Status)
                .HasDefaultValue((int)TimeLogStatus.Processing)
                .HasColumnName("status");
            entity.Property(e => e.RegularMinutes)
                .HasColumnName("regular_minutes")
                .HasDefaultValue(0);

            entity.Property(e => e.DailyOtMinutes)
                .HasColumnName("daily_ot_minutes")
                .HasDefaultValue(0);

            entity.Property(e => e.WeeklyOtMinutes)
                .HasColumnName("weekly_ot_minutes")
                .HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ShiftAssignment).WithMany(p => p.TimeLogs)
                .HasForeignKey(d => d.ShiftAssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_timelogs_assignment");
        });

        // Cau hinh bang shift_applications (Don dang ky nhan ca tu Cho Ca)
        modelBuilder.Entity<ShiftApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shift_applications_pkey");
            entity.ToTable("shift_applications");

            // Rang buoc: 1 nhan vien chi duoc dang ky 1 lan cho moi ca
            entity.HasIndex(e => new { e.ShiftId, e.UserId }, "uq_application_user_shift").IsUnique();
            entity.HasIndex(e => e.ShiftId, "idx_applications_shift");
            entity.HasIndex(e => e.UserId, "idx_applications_user");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Status).HasDefaultValue((int)ShiftApplicationStatus.Pending).HasColumnName("status");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.AppliedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("applied_at");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewNote).HasColumnName("review_note");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");

            entity.HasOne(e => e.Shift).WithMany()
                .HasForeignKey(e => e.ShiftId)
                .HasConstraintName("fk_applications_shift");
        });

        // Cau hinh bang shift_swap_requests (Yeu cau doi ca)
        modelBuilder.Entity<ShiftSwapRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shift_swap_requests_pkey");
            entity.ToTable("shift_swap_requests");

            entity.HasIndex(e => new { e.SourceUserId, e.Status }, "idx_swap_source_user");
            entity.HasIndex(e => new { e.TargetUserId, e.Status }, "idx_swap_target_user");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.SourceAssignmentId).HasColumnName("source_assignment_id");
            entity.Property(e => e.SourceUserId).HasColumnName("source_user_id");
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id");
            entity.Property(e => e.Status).HasDefaultValue((int)SwapRequestStatus.PendingTarget).HasColumnName("status");
            entity.Property(e => e.Reason).HasMaxLength(500).HasColumnName("reason");
            entity.Property(e => e.RejectReason).HasMaxLength(500).HasColumnName("reject_reason");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");

            entity.HasOne(e => e.SourceAssignment).WithMany()
                .HasForeignKey(e => e.SourceAssignmentId)
                .HasConstraintName("fk_swap_source_assignment");
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeletedEntity).IsAssignableFrom(entity.ClrType))
            {
                // Cấu hình Query Filter: x => !x.IsDeleted
                modelBuilder.Entity(entity.ClrType)
                    .HasQueryFilter(GenerateIsDeletedExpression(entity.ClrType));
            }
        }

        OnModelCreatingPartial(modelBuilder);
        
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");

            entity.ToTable("outbox_messages");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Content)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Error).HasColumnName("error");
            entity.Property(e => e.OccurredOn).HasColumnName("occurred_on");
            entity.Property(e => e.ProcessedOn).HasColumnName("processed_on");
            entity.Property(e => e.Type)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            
            entity.HasIndex(e => e.ProcessedOn, "ix_outbox_messages_processed_on")
                  .HasFilter("processed_on IS NULL");
        });

        modelBuilder.Entity<FranchiseApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("franchise_applications_pkey");

            entity.ToTable("franchise_applications");

            entity.HasIndex(e => e.Email, "idx_franchise_app_email");
            entity.HasIndex(e => e.IdentityNumber, "idx_franchise_app_identity");
            entity.HasIndex(e => e.ReviewedBy, "idx_franchise_app_reviewed_by");
            entity.HasIndex(e => e.Status, "idx_franchise_app_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");

            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.IdentityNumber)
                .HasMaxLength(20)
                .HasColumnName("identity_number");

            entity.Property(e => e.ProvinceId)
                .HasMaxLength(50)
                .HasColumnName("province_id");
            entity.Property(e => e.DistrictId)
                .HasMaxLength(50)
                .HasColumnName("district_id");
            entity.Property(e => e.WardId)
                .HasMaxLength(50)
                .HasColumnName("ward_id");
            entity.Property(e => e.StoreAddress).HasColumnName("store_address");
            entity.Property(e => e.StoreLatitude)
                .HasPrecision(10, 8)
                .HasColumnName("store_latitude");
            entity.Property(e => e.StoreLongitude)
                .HasPrecision(11, 8)
                .HasColumnName("store_longitude");
            entity.Property(e => e.StoreSizeSqm)
                .HasPrecision(10, 2)
                .HasColumnName("store_size_sqm");

            // LandCertificates → PostgreSQL TEXT[]
            entity.Property(e => e.LandCertificates)
                .HasColumnName("land_certificates");

            entity.Property(e => e.StoreFrontUrl)
                .HasMaxLength(500)
                .HasColumnName("store_front_url");
            entity.Property(e => e.StoreFrontPublicId)
                .HasMaxLength(100)
                .HasColumnName("store_front_public_id");

            entity.Property(e => e.Status)
                .HasDefaultValue(FranchiseApplicationStatus.Pending)
                .HasColumnName("status");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");

            // Navigation: 1 Application → N Contracts
            entity.HasMany(e => e.Contracts)
                .WithOne(c => c.Application)
                .HasForeignKey(c => c.ApplicationId)
                .HasConstraintName("fk_contract_application");
        });

        modelBuilder.Entity<FranchiseContract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("franchise_contracts_pkey");

            entity.ToTable("franchise_contracts");

            entity.HasIndex(e => e.ApplicationId, "idx_franchise_contract_app");
            entity.HasIndex(e => e.DocusealSubmissionId, "idx_franchise_contract_docuseal");
            entity.HasIndex(e => e.Status, "idx_franchise_contract_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");

            entity.Property(e => e.DocusealSubmissionId).HasColumnName("docuseal_submission_id");
            entity.Property(e => e.ContractPdfUrl)
                .HasMaxLength(500)
                .HasColumnName("contract_pdf_url");

            entity.Property(e => e.DepositAmount)
                .HasPrecision(15, 2)
                .HasColumnName("deposit_amount");
            entity.Property(e => e.DepositStatus)
                .HasDefaultValue(DepositStatus.AwaitingPayment)
                .HasColumnName("deposit_status");
            entity.Property(e => e.FranchiseFee)
                .HasPrecision(15, 2)
                .HasColumnName("franchise_fee");
            entity.Property(e => e.RevenueSharePercentage)
                .HasPrecision(5, 2)
                .HasColumnName("revenue_share_percentage");

            entity.Property(e => e.Status)
                .HasDefaultValue(ContractStatus.Draft)
                .HasColumnName("status");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");

            entity.Property(e => e.SetupStatus)
                .HasDefaultValue(ContractSetupStatus.Design)
                .HasColumnName("setup_status");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
        });
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