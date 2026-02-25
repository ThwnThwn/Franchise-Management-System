using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkCreateFromTemplate
{
    // Lay gio lam viec tu Template (Ca Sang/Chieu/Toi)
    // Quet tung ngay tu startDate den endDate de tao ca Draft
    public class BulkCreateFromTemplateHandler
        : IRequestHandler<BulkCreateFromTemplateCommand, int>
    {
        private readonly IGenericRepository<Shift> _genericRepository;
        private readonly IGenericRepository<ShiftTemplate> _templateRepository;
        private readonly IShiftRepository _shiftRepository;

        public BulkCreateFromTemplateHandler(
            IGenericRepository<Shift> genericRepository,
            IGenericRepository<ShiftTemplate> templateRepository,
            IShiftRepository shiftRepository)
        {
            _genericRepository = genericRepository;
            _templateRepository = templateRepository;
            _shiftRepository = shiftRepository;
        }

        public async Task<int> Handle(
            BulkCreateFromTemplateCommand request, CancellationToken cancellationToken)
        {
            // Validate 1: Template phai ton tai trong DB
            var template = await _templateRepository.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException(
                    $"Khong tim thay Mau ca lam viec voi ID: {request.TemplateId}");

            // Validate 2: Template phai dang hoat dong (IsActive = true)
            if (template.IsActive == false)
                throw new InvalidOperationException("Mau ca nay da bi tat, khong the su dung.");

            // Validate 3: startDate phai truoc hoac bang endDate
            if (request.StartDate > request.EndDate)
                throw new ArgumentException("Ngay bat dau phai truoc hoac bang ngay ket thuc.");

            // Validate 4: khong cho tao ca cho ngay qua khu
            if (request.StartDate < DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("Khong the tao ca cho ngay trong qua khu.");

            // Validate 5: so luong nhan vien phai it nhat 1
            if (request.RequiredStaffCount < 1)
                throw new ArgumentException("So luong nhan vien phai lon hon hoac bang 1.");

            // Validate 6: gioi han khoang thoi gian toi da 90 ngay de bao ve DB
            var daysDiff = request.EndDate.DayNumber - request.StartDate.DayNumber;
            if (daysDiff > 90)
                throw new ArgumentException("Khoang thoi gian toi da la 90 ngay.");

            // Lay ca da ton tai trong khoang ngay de check trung (1 query duy nhat)
            var existingShifts = await _shiftRepository.GetShiftsByDateRangeAsync(
                request.StoreId, request.StartDate, request.EndDate, cancellationToken);

            // Dung HashSet de lookup nhanh O(1)
            var existingSet = existingShifts
                .Select(s => (s.ShiftDate, s.StartTime, s.EndTime))
                .ToHashSet();

            // Quet MOI NGAY trong khoang (khac BulkCreateManual khong co filter daysOfWeek)
            var shiftsToCreate = new List<Shift>();
            var current = request.StartDate;

            while (current <= request.EndDate)
            {
                // Bo qua neu da ton tai ca cung gio tai cung Store (khong throw loi)
                if (!existingSet.Contains((current, template.StartTime, template.EndTime)))
                {
                    shiftsToCreate.Add(new Shift
                    {
                        Id = Guid.NewGuid(),
                        StoreId = request.StoreId,
                        ShiftTemplateId = template.Id,
                        ShiftDate = current,
                        StartTime = template.StartTime,
                        EndTime = template.EndTime,
                        RequiredStaffCount = request.RequiredStaffCount,
                        BreakDurationMinutes = template.BreakDurationMinutes ?? 0,
                        Status = (int)ShiftStatus.Draft,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        LockVersion = 1
                    });
                }

                current = current.AddDays(1);
            }

            // Bulk insert 1 lan duy nhat
            if (shiftsToCreate.Any())
            {
                await _genericRepository.AddRangeAsync(shiftsToCreate);
            }

            // Tra ve so ca thuc te duoc tao (khong tinh ca bi skip do trung)
            return shiftsToCreate.Count;
        }
    }
}
