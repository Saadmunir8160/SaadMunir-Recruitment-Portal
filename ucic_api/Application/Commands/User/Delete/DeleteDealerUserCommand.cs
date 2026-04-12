using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User.Delete
{
    public class DeleteDealerUserCommand : IRequest<Response<DeleteDealerUserCommand>>
    {
        [Required(ErrorMessage = "DealerId is required")]
        public int DealerId { get; set; }
    }

    public class DeleteDealerUserCommandHandler : IRequestHandler<DeleteDealerUserCommand, Response<DeleteDealerUserCommand>>
    {
        private readonly IIdentityService _identityService;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;

        public DeleteDealerUserCommandHandler(
            IIdentityService identityService,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository)
        {
            _identityService = identityService;
            _dealerQueryRepository = dealerQueryRepository;
            _dealerCommandRepository = dealerCommandRepository;
        }

        public async Task<Response<DeleteDealerUserCommand>> Handle(DeleteDealerUserCommand request, CancellationToken cancellationToken)
        {
            // Get the dealer entity
            var dealer = await _dealerQueryRepository.GetByIdAsync(request.DealerId);
            if (dealer == null)
            {
                return new Response<DeleteDealerUserCommand>
                {
                    Success = false,
                    Message = "Dealer not found",
                    Data = request
                };
            }

            // Use hard delete - cascade will handle related records (products, drivers, vehicles, addresses, limits)
            // Note: If dealer has orders, this will fail due to RESTRICT constraint
            try
            {
                await _dealerCommandRepository.HardDeleteAsync(dealer);
                
                // Optionally delete the user from Identity system
                // var userDeleteResult = await _identityService.DeleteUserAsync(dealer.UserId);

                return new Response<DeleteDealerUserCommand>
                {
                    Success = true,
                    Message = $"Dealer user deleted successfully for ID: {request.DealerId}",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                return new Response<DeleteDealerUserCommand>
                {
                    Success = false,
                    Message = $"Cannot delete dealer: {ex.Message}. Dealer may have existing orders.",
                    Data = request
                };
            }
        }
    }
}
