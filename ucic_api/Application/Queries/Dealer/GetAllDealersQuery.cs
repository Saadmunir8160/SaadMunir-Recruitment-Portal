using Application.Common.Extensions;
using Application.DTOs;
using Application.Services;
using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.Dealer
{
    public class GetAllDealersQuery : IRequest<PaginatedResponse<DealerDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllDealersQueryHandler : IRequestHandler<GetAllDealersQuery, PaginatedResponse<DealerDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Dealer> _repository;
        private readonly IUserQueryService _userQueryService;
        
        public GetAllDealersQueryHandler(
            IQueryRepository<Domain.Entities.Dealer> repository,
            IUserQueryService userQueryService)
        {
            _repository = repository;
            _userQueryService = userQueryService;
        }

        public async Task<PaginatedResponse<DealerDTO>> Handle(GetAllDealersQuery request, CancellationToken cancellationToken)
        {
            var dealers = await _repository.GetAllAsync();
            
            // Apply filters
            if (request.IsActive.HasValue)
            {
                dealers = dealers.Where(d => d.IsActive == request.IsActive.Value).ToList();
            }
            
            var dealerDTOs = new List<DealerDTO>();
            
            foreach (var dealer in dealers)
            {
                var userInfo = await _userQueryService.GetUserInfoAsync(dealer.UserId);
                
                var dealerDTO = new DealerDTO()
                {
                    DealerId = dealer.DealerId,
                    UserId = dealer.UserId,
                    DealerName = dealer.DealerName,
                    CreditLimit = dealer.CreditLimit,
                    CurrentBalance = dealer.CurrentBalance,
                    Ln_ID = dealer.Ln_ID,
                    IsActive = dealer.IsActive,
                    CreatedDate = dealer.CreatedDate,
                    ModifiedDate = dealer.ModifiedDate,
                    // User information from Identity
                    Email = userInfo?.Email,
                    PhoneNumber = userInfo?.PhoneNumber,
                    FullName = userInfo?.FullName,
                    UserName = userInfo?.UserName
                };
                
                dealerDTOs.Add(dealerDTO);
            }
            
            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                dealerDTOs = dealerDTOs.Where(d =>
                    (!string.IsNullOrEmpty(d.DealerName) && d.DealerName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.FullName) && d.FullName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.Email) && d.Email.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.PhoneNumber) && d.PhoneNumber.Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.UserName) && d.UserName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.Ln_ID) && d.Ln_ID.ToLower().Contains(searchLower))
                ).ToList();
            }

            // Manual pagination since we can't use EF async operations on List<DealerDTO>
            var totalCount = dealerDTOs.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            
            var pagedData = dealerDTOs
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedResponse<DealerDTO>
            {
                Data = pagedData,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                },
                Success = true,
                Message = "Data retrieved successfully."
            };
        }
    }
}
