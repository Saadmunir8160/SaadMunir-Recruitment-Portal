using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;

namespace Application.Commands.Auth
{
    public class AuthCommand : IRequest<AuthResponseDTO>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class AuthCommandHandler : IRequestHandler<AuthCommand, AuthResponseDTO>
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IIdentityService _identityService;


        public AuthCommandHandler(IIdentityService identityService, ITokenGenerator tokenGenerator)
        {
            _identityService = identityService;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResponseDTO> Handle(AuthCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.SigninUserAsync(request.Email, request.Password);

            if (!result)
            {
                throw new BadRequestException("Invalid username or password");
            }

            var (userId, fullName, userName, email, phoneNumber, roles) = await _identityService.GetUserDetailsAsync(await _identityService.GetUserIdAsync(request.Email));

            var rolesList = roles?.ToList() ?? new List<string>();

            // JWT still contains every role claim. Expose a single Role for admin UI: prefer recruitment RMS roles.
            string[] priority =
            {
                "HR Section Head", "HR Manager", "HR", "hr", "Recruiter", "HRSupervisor",
                "HiringManager", "Admin", "User", "Finance", "Purchase", "Sale", "Dealer", "DealerDriver", "Candidate"
            };

            var lookup = new HashSet<string>(rolesList, StringComparer.OrdinalIgnoreCase);
            var primaryRole = priority.FirstOrDefault(p => lookup.Contains(p))
                ?? rolesList.FirstOrDefault()
                ?? string.Empty;

            string token = _tokenGenerator.GenerateJWTToken((userId, userName, roles), fullName);

            return new AuthResponseDTO()
            {
                UserId = userId,
                Name = fullName,
                Role = primaryRole,
                Roles = rolesList,
                Token = token
            };
        }
    }
}
