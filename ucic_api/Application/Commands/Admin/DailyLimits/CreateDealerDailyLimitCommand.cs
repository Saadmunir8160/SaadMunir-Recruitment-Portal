using MediatR;
using Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Admin.DailyLimits
{
    public class CreateDealerDailyLimitCommand : IRequest<Response<object>>
    {
        public int? DealerID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string? LimitType { get; set; }
        
        [Required]
        public decimal? LimitValue { get; set; }
        
        [Required]
        public DateTime? EffectiveDate { get; set; }
    }
}