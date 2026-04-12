using Application.Common.Interfaces;
using Application.DTOs;
using Application.DTOs.Dealer;
using MediatR;

namespace Application.Commands.Dealer
{
    public class ChangeDealerPasswordCommand : IRequest<Response<string>>
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangeDealerPasswordCommandHandler : IRequestHandler<ChangeDealerPasswordCommand, Response<string>>
    {
        private readonly IIdentityService _identityService;

        public ChangeDealerPasswordCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Response<string>> Handle(ChangeDealerPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate passwords match
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new Response<string>
                    {
                        Success = false,
                        Message = "New password and confirmation password do not match."
                    };
                }

                // Get current user
                var currentUserId = _identityService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return new Response<string>
                    {
                        Success = false,
                        Message = "User not authenticated."
                    };
                }

                // Change password using IdentityService
                var result = await _identityService.ChangePasswordAsync(currentUserId, request.CurrentPassword, request.NewPassword);
                if (!result)
                {
                    return new Response<string>
                    {
                        Success = false,
                        Message = "Password change failed. Please check your current password and try again."
                    };
                }

                return new Response<string>
                {
                    Success = true,
                    Message = "Password changed successfully.",
                    Data = "Password updated"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = $"An error occurred while changing password: {ex.Message}"
                };
            }
        }
    }
}