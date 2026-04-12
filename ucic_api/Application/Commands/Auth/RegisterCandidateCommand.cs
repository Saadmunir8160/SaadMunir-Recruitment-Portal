using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Auth
{
    public class RegisterCandidateCommand : IRequest<AuthResponseDTO>
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class RegisterCandidateCommandHandler : IRequestHandler<RegisterCandidateCommand, AuthResponseDTO>
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenGenerator _tokenGenerator;

        public RegisterCandidateCommandHandler(IIdentityService identityService, ITokenGenerator tokenGenerator)
        {
            _identityService = identityService;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResponseDTO> Handle(RegisterCandidateCommand request, CancellationToken cancellationToken)
        {
            if (request.Password != request.ConfirmPassword)
                throw new BadRequestException("Passwords do not match.");

            // Use email as the username for candidate self-registration
            var (isSucceed, userIdOrError) = await _identityService.CreateUserAsync(
                userName: request.Email,
                password: request.Password,
                email: request.Email,
                fullName: request.FullName,
                phoneNO: request.PhoneNumber,
                roles: "Candidate"
            );

            if (!isSucceed)
                throw new BadRequestException(userIdOrError);

            var (userId, fullName, userName, email, phoneNumber, roles) =
                await _identityService.GetUserDetailsAsync(userIdOrError);

            string token = _tokenGenerator.GenerateJWTToken((userId, userName, roles), fullName);

            return new AuthResponseDTO
            {
                UserId = userId,
                Name = fullName,
                Role = roles.FirstOrDefault() ?? "Candidate",
                Token = token
            };
        }
    }
}
