using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Order
{
    public class ValidateCouponQuery : IRequest<ValidateCouponDTO>
    {
        public string couponCode {  get; set; }
        public long coverageAreaId {  get; set; }
    }

    public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponDTO>
    {
        private readonly IQueryRepository<Domain.Entities.Promotion> _queryRepository;

        public ValidateCouponQueryHandler(IQueryRepository<Domain.Entities.Promotion> queryRepository)
        {
            _queryRepository = queryRepository;
        }
        public async Task<ValidateCouponDTO> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
        {
            //var filters = new Dictionary<string, object>
            //  {
            //      { nameof(Domain.Entities.Promotion.Code), request.couponCode }
            //  };
            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Promotion.Code), request.couponCode },
                { nameof(Domain.Entities.Promotion.CoverageAreaId), request.coverageAreaId }
            };

            var Result = await _queryRepository.GetByColumnsAsync(filters);

            var coupon = Result.FirstOrDefault();
            if (request.couponCode.Equals(Result.FirstOrDefault()?.Code))
            {
                if (coupon != null &&
                    coupon.ValidFrom.Date <= DateTime.Now.Date &&
                    coupon.ValidTo.Date >= DateTime.Now.Date)
                {
                    return new ValidateCouponDTO()
                    {
                        isValid = true,
                        couponCode = coupon.Code,
                        discountPercentage = coupon.DiscountPercentage,
                        PromotionID = coupon.PromotionId
                    };
                }
            }

            return new ValidateCouponDTO()
            {
                isValid = false,
                couponCode = coupon?.Code,
                discountPercentage = 0,
                PromotionID = 0
            };
        }
    }
}
