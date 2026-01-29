using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FranchiseService.Application.Features.Stores.Dtos
{
    public class StoreDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ImageUrl { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsOpenNow { get; set; }
    }

    public class AvailabilityResult
    {
        public bool IsAvailable { get; set; }
        public string? Message { get; set; }
    }
}