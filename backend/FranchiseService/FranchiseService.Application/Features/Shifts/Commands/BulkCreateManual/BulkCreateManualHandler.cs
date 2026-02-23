using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkCreateManual
{
    // Quet ngay tu startDate den endDate theo daysOfWeek
    // Bo qua ca bi trung (cung Store, Ngay, Gio), tao hang loat ca Draft
    public class BulkCreateManualHandler : IRequestHandler<BulkCreateManualCommand, int>
    {
        private readonly IGenericRepository<Shift> _genericRepository;
        private readonly IShiftRepository _shiftRepository;

        public BulkCreateManualHandler(
            IGenericRepository<Shift> genericRepository,
            IShiftRepository shiftRepository)
        {
            _genericRepository = genericRepository;
            _shiftRepository = shiftRepository;
        }

        public async Task<int> Handle(
            BulkCreateManualCommand request, CancellationToken cancellationToken)
        {
            // Validate 1: startDate phai truoc hoac bang endDate
            if (request.StartDate > request.EndDate)
                throw new ArgumentException("Ngay bat dau phai truoc hoac bang ngay ket thuc.");

            // Validate 2: khong cho tao ca cho ngay qua khu
            if (request.StartDate < DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("Khong the tao ca cho ngay trong qua khu.");

            // Validate 3: phai chon it nhat 1 ngay trong tuan
            if (!request.DaysOfWeek.Any())
                throw new ArgumentException("Phai chon it nhat 1 ngay trong tuan.");

            // Validate 4: daysOfWeek chi chap nhan gia tri 0 (Sun) den 6 (Sat)
            if (request.DaysOfWeek.Any(d => d < 0 || d > 6))
                throw new ArgumentException("daysOfWeek chi chap nhan gia tri tu 0 (Chu Nhat) den 6 (Thu Bay).");

            // Validate 5: so luong nhan vien phai it nhat 1
            if (request.RequiredStaffCount < 1)
                throw new ArgumentException("So luong nhan vien phai lon hon hoac bang 1.");

            // Validate 6: thoi gian nghi khong the la so am
            if (request.BreakDurationMinutes < 0)
                throw new ArgumentException("Thoi gian nghi khong the la so am.");

            // Validate 7: gioi han khoang thoi gian toi da 90 ngay de bao ve DB
            var daysDiff = request.EndDate.DayNumber - request.StartDate.DayNumber;
            if (daysDiff > 90)
                throw new ArgumentException("Khoang thoi gian toi da la 90 ngay.");

            // Lay tat ca ca da ton tai trong khoang ngay de check trung (1 query duy nhat)
            var existingShifts = await _shiftRepository.GetShiftsByDateRangeAsync(
                request.StoreId, request.StartDate, request.EndDate, cancellationToken);

            // Dung HashSet de lookup nhanh O(1) thay vi duyet mang O(n)
            var existingSet = existingShifts
                .Select(s => (s.ShiftDate, s.StartTime, s.EndTime))
                .ToHashSet();

            // Quet tung ngay tu startDate den endDate
            var shiftsToCreate = new List<Shift>();
            var current = request.StartDate;

            while (current <= request.EndDate)
            {
                // Kiem tra ngay hien tai co nam trong daysOfWeek khong
                // C# DayOfWeek: 0=Sunday, 1=Monday, ... 6=Saturday (khop voi FE)
                int dow = (int)current.DayOfWeek;

                if (request.DaysOfWeek.Contains(dow))
                {
                    // Bo qua neu da ton tai ca cung gio tai cung Store (khong throw loi)
                    if (!existingSet.Contains((current, request.StartTime, request.EndTime)))
                    {
                        shiftsToCreate.Add(new Shift
                        {
                            Id = Guid.NewGuid(),
                            StoreId = request.StoreId,
                            ShiftDate = current,
                            StartTime = request.StartTime,
                            EndTime = request.EndTime,
                            RequiredStaffCount = request.RequiredStaffCount,
                            BreakDurationMinutes = request.BreakDurationMinutes,
                            Note = request.Note,
                            Status = (int)ShiftStatus.Draft,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            LockVersion = 1
                        });
                    }
                }

                current = current.AddDays(1);
            }

            // Bulk insert 1 lan duy nhat thay vi insert tung cai
            if (shiftsToCreate.Any())
            {
                await _genericRepository.AddRangeAsync(shiftsToCreate);
            }

            // Tra ve so ca thuc te duoc tao (khong tinh ca bi skip do trung)
            return shiftsToCreate.Count;
        }
    }
}
