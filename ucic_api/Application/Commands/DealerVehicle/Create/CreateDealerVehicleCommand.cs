using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerVehicle
{
    public class CreateDealerVehicleCommand : IRequest<Response<int>>
    {
        public required CreateDealerVehicleDTO DealerVehicle { get; set; }
    }

    public class CreateDealerVehicleCommandHandler : IRequestHandler<CreateDealerVehicleCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _repository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;
        private readonly IIdentityService _identityService;
        
        public CreateDealerVehicleCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> repository,
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository,
            IIdentityService identityService)
        {
            _repository = repository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }
        
        public async Task<Response<int>> Handle(CreateDealerVehicleCommand request, CancellationToken cancellationToken)
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

            if (!string.IsNullOrEmpty(request.DealerVehicle.Ln_ID))
            {
                var vehicleExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.DealerVehicle.Ln_ID), request.DealerVehicle.Ln_ID);
                if (vehicleExists)
                {
                    return new Response<int>
                    {
                        Success = false,
                        Message = $"Vehicle LN ID '{request.DealerVehicle.Ln_ID}' already exists."
                    };
                }
            }

            var entity = new Domain.Entities.DealerVehicle
            {
                DealerID = currentDealerId.Value, // Use centralized dealer ID
                PlateNumber = request.DealerVehicle.PlateNumber,
                Type = request.DealerVehicle.Type,
                Capacity = request.DealerVehicle.Capacity,
                RegistrationDate = request.DealerVehicle.RegistrationDate,
                RegistrationExpiryDate = request.DealerVehicle.RegistrationExpiryDate,
                InsuranceExpiryDate = request.DealerVehicle.InsuranceExpiryDate,
                Ln_ID = request.DealerVehicle.Ln_ID,
                RegistrationNumber = request.DealerVehicle.RegistrationNumber,
                VehicleType = request.DealerVehicle.Type, // Map to legacy field for backward compatibility
                IsActive = request.DealerVehicle.IsActive,
                CreatedDate = DateTime.UtcNow
            };
                
            await _repository.AddAsync(entity);

            return new Response<int> 
            { 
                Success = true, 
                Data = entity.VehicleID, 
                Message = "Dealer vehicle created successfully." 
            };
        }
    }
}
