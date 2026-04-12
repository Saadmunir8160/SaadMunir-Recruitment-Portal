using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.User
{
    public class GetUserQuery : IRequest<PaginatedResponse<UserResponseDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, PaginatedResponse<UserResponseDTO>>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetUserQueryHandler> _logger;

        public GetUserQueryHandler(IIdentityService identityService, ILogger<GetUserQueryHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<PaginatedResponse<UserResponseDTO>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            
            // Get all users with their roles
            var usersWithRoles = await _identityService.GetAllUsersDetailsAsync();
            
            // Debug logging
            _logger.LogInformation($"GetAllUsersDetailsAsync returned {usersWithRoles?.Count ?? 0} users");
            
            // Debug: Check if we're getting users
            if (usersWithRoles == null || !usersWithRoles.Any())
            {
                _logger.LogWarning("No users found in GetAllUsersDetailsAsync");
                // Return empty paginated response instead of null
                return new PaginatedResponse<UserResponseDTO>
                {
                    Data = new List<UserResponseDTO>(),
                    Metadata = new PaginationMetadata
                    {
                        CurrentPage = parameters.PageNumber,
                        PageSize = parameters.PageSize,
                        TotalCount = 0,
                        TotalPages = 0
                    },
                    Success = true,
                    Message = "No users found."
                };
            }
            
            var query = usersWithRoles.Select(x => new UserResponseDTO()
            {
                Id = x.id,
                FullName = x.fullName,
                UserName = x.userName,
                Email = x.email,
                Phone = x.phoneNumber,
                Roles = x.roles.ToArray()
            });
            
            var result = await query.ToPaginatedResponseAsync(parameters);
            _logger.LogInformation($"Returning {result.Data.Count} users out of {result.Metadata.TotalCount} total");
            
            return result;
        }
    }
}
