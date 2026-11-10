using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FranchiseService.Application.Dtos.Image
{
    public class ImageUploadResult
    {
        public string Url { get; set; } = null!;
        public string PublicId { get; set; } = null!;
    }
}
