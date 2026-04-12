using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class CreateDealerDriverCommand : IRequest<Response<int>>
    {
        public CreateDealerDriverDTO DealerDriver { get; set; } = new();
    }

    public class CreateDealerDriverCommandHandler : IRequestHandler<CreateDealerDriverCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDriver> _repository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<CreateDealerDriverCommandHandler> _logger;

        public CreateDealerDriverCommandHandler(
            ICommandRepository<Domain.Entities.DealerDriver> repository,
            IQueryRepository<Domain.Entities.DealerDriver> queryRepository,
            IIdentityService identityService,
            ILogger<CreateDealerDriverCommandHandler> logger)
        {
            _repository = repository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<int>> Handle(CreateDealerDriverCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<int>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            if (!string.IsNullOrEmpty(request.DealerDriver.Ln_ID))
            {
                var driverExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.DealerDriver.Ln_ID), request.DealerDriver.Ln_ID);
                if (driverExists)
                {
                    return new Response<int>
                    {
                        Success = false,
                        Message = $"Driver LN ID '{request.DealerDriver.Ln_ID}' already exists."
                    };
                }
            }

            var entity = new Domain.Entities.DealerDriver
            {
                DealerID = currentDealerId.Value, // Use centralized dealer ID
                UserId = request.DealerDriver.UserId,
                Ln_ID = request.DealerDriver.Ln_ID,
                IqamaNumber = request.DealerDriver.IqamaNumber,
                IsActive = request.DealerDriver.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _identityService.GetCurrentUserId() ?? "System"
            };

            var driverId = await _repository.AddAsync(entity);
            entity.DriverID = driverId;

            _logger.LogInformation("Dealer driver created successfully for dealer {DealerId} with ID {DriverId}", 
                currentDealerId.Value, driverId);

            return new Response<int> 
            { 
                Success = true, 
                Data = entity.DriverID, 
                Message = "Dealer driver created successfully." 
            };
        }
    }
}