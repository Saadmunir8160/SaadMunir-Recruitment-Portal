using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.DealerArea
{
    public class CreateDealerAreaForAdminCommand : IRequest<Response<bool>>
    {
        public string AreaName { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
    }

    public class CreateDealerAreaForAdminCommandHandler : IRequestHandler<CreateDealerAreaForAdminCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerArea> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<CreateDealerAreaForAdminCommandHandler> _logger;

        public CreateDealerAreaForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerArea> commandRepository,
            IQueryRepository<Domain.Entities.DealerArea> queryRepository,
            IIdentityService identityService,
            ILogger<CreateDealerAreaForAdminCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(CreateDealerAreaForAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if AreaCode already exists
                var areaExists = await _queryRepository.ValueExistsAsync(
                    nameof(Domain.Entities.DealerArea.AreaCode), 
                    request.AreaCode);
                
                if (areaExists)
                {
                    return new Response<bool>
                    {
                        Success = false,
                        Message = $"Area code '{request.AreaCode}' already exists."
                    };
                }

                var area = new Domain.Entities.DealerArea
                {
                    AreaName = request.AreaName,
                    AreaCode = request.AreaCode,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _identityService.GetCurrentUserId()
                };

                await _commandRepository.AddAsync(area);

                _logger.LogInformation("Admin created dealer area {AreaId} successfully", area.AreaID);

                return new Response<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Dealer area created successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dealer area");
                return new Response<bool>
                {
                    Success = false,
                    Message = $"Error creating dealer area: {ex.Message}"
                };
            }
        }
    }

    public class UpdateDealerAreaForAdminCommand : IRequest<Response<bool>>
    {
        public int Id { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateDealerAreaForAdminCommandHandler : IRequestHandler<UpdateDealerAreaForAdminCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerArea> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateDealerAreaForAdminCommandHandler> _logger;

        public UpdateDealerAreaForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerArea> commandRepository,
            IQueryRepository<Domain.Entities.DealerArea> queryRepository,
            IIdentityService identityService,
            ILogger<UpdateDealerAreaForAdminCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(UpdateDealerAreaForAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _queryRepository.GetByIdAsync(request.Id);
                if (entity == null)
                {
                    return new Response<bool>
                    {
                        Success = false,
                        Message = "Dealer area not found."
                    };
                }

                entity.AreaName = request.AreaName;
                entity.AreaCode = request.AreaCode;
                entity.IsActive = request.IsActive;
                entity.ModifiedDate = DateTime.UtcNow;
                entity.ModifiedBy = _identityService.GetCurrentUserId();

                await _commandRepository.UpdateAsync(entity);

                _logger.LogInformation("Admin updated dealer area {AreaId} successfully", request.Id);

                return new Response<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Dealer area updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dealer area {AreaId}", request.Id);
                return new Response<bool>
                {
                    Success = false,
                    Message = $"Error updating dealer area: {ex.Message}"
                };
            }
        }
    }

    public class DeleteDealerAreaForAdminCommand : IRequest<Response<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteDealerAreaForAdminCommandHandler : IRequestHandler<DeleteDealerAreaForAdminCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerArea> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _queryRepository;
        private readonly ILogger<DeleteDealerAreaForAdminCommandHandler> _logger;

        public DeleteDealerAreaForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerArea> commandRepository,
            IQueryRepository<Domain.Entities.DealerArea> queryRepository,
            ILogger<DeleteDealerAreaForAdminCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(DeleteDealerAreaForAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _queryRepository.GetByIdAsync(request.Id);
                if (entity == null)
                {
                    return new Response<bool>
                    {
                        Success = false,
                        Message = "Dealer area not found."
                    };
                }

                await _commandRepository.DeleteAsync(entity);

                _logger.LogInformation("Admin deleted dealer area {AreaId} successfully", request.Id);

                return new Response<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Dealer area deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dealer area {AreaId}", request.Id);
                return new Response<bool>
                {
                    Success = false,
                    Message = $"Error deleting dealer area: {ex.Message}"
                };
            }
        }
    }
}
