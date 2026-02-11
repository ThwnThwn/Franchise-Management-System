using System.Linq.Expressions;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.ApplicationDbContext;

public partial class FranchiseDbContext : DbContext, IFranchiseDbContext
{
    private readonly UpdateTimestampInterceptor _updateInterceptor;

    public FranchiseDbContext()
    {
    }

    public FranchiseDbContext(DbContextOptions<FranchiseDbContext> options, UpdateTimestampInterceptor updateTimestampInterceptor)
        : base(options)
    {
        _updateInterceptor = updateTimestampInterceptor;
    }

    public virtual DbSet<Franchisee> Franchisees { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<ShiftAssignment> ShiftAssignments { get; set; }

    public virtual DbSet<ShiftRule> ShiftRules { get; set; }

    public virtual DbSet<ShiftTemplate> ShiftTemplates { get; set; }

    public virtual DbSet<StaffAvailability> StaffAvailabilities { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreHour> StoreHours { get; set; }

    public virtual DbSet<TimeLog> TimeLogs { get; set; }

    public DbSet<T> Set<T>() where T : class, IEntity => base.Set<T>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateInterceptor);
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
                .HasDefaultValue(0)
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
                .HasDefaultValue(0)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

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
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasDefaultValue(0)
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
                .HasDefaultValue(0)
                .HasColumnName("status");
            entity.Property(e => e.TotalWorkMinutes)
                .HasDefaultValue(0)
                .HasColumnName("total_work_minutes");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ShiftAssignment).WithMany(p => p.TimeLogs)
                .HasForeignKey(d => d.ShiftAssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_timelogs_assignment");
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