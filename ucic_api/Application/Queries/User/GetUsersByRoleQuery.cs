using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.User
{
    public class GetUsersByRoleQuery : IRequest<List<UserDropdownDTO>>
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, List<UserDropdownDTO>>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetUsersByRoleQueryHandler> _logger;

        public GetUsersByRoleQueryHandler(IIdentityService identityService, ILogger<GetUsersByRoleQueryHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<List<UserDropdownDTO>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
        {
            // Get users by role
            var usersWithRoles = await _identityService.GetUsersByRoleAsync(request.RoleName);
            
            // Debug logging
            _logger.LogInformation($"GetUsersByRoleAsync returned {usersWithRoles?.Count ?? 0} users for role '{request.RoleName}'");
            
            // Debug: Check if we're getting users
            if (usersWithRoles == null || !usersWithRoles.Any())
            {
                _logger.LogWarning($"No users found for role '{request.RoleName}'");
                return new List<UserDropdownDTO>();
            }
            
            var result = usersWithRoles.Select(x => new UserDropdownDTO()
            {
                Id = x.id,
                FullName = x.fullName
            }).ToList();
            
            _logger.LogInformation($"Returning {result.Count} users for role '{request.RoleName}'");
            
            return result;
        }
    }
} 