using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ValidateCouponDTO
    {
        public bool isValid { get; set; }
        public string? couponCode { get; set; }
        public decimal discountPercentage { get; set; }
        public long PromotionID { get; set; }
    }
}
