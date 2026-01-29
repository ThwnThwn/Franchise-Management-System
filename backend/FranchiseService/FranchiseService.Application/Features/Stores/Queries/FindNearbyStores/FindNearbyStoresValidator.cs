using FluentValidation;

namespace FranchiseService.Application.Features.Stores.Queries.FindNearbyStores
{
    public class FindNearbyStoresValidator : AbstractValidator<FindNearbyStoresQuery>
    {
        public FindNearbyStoresValidator()
        {
            RuleFor(x => x.UserLat)
                .InclusiveBetween(-90, 90).WithMessage("Vĩ độ (Latitude) không hợp lệ");

            RuleFor(x => x.UserLong)
                .InclusiveBetween(-180, 180).WithMessage("Kinh độ (Longitude) không hợp lệ");

            RuleFor(x => x.RadiusKm)
                .GreaterThan(0).WithMessage("Bán kính tìm kiếm phải lớn hơn 0")
                .LessThanOrEqualTo(50).WithMessage("Chỉ hỗ trợ tìm kiếm trong bán kính 50km");

            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Limit phải lớn hơn 0");
        }
    }
}