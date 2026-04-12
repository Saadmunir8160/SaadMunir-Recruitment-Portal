using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerArea
{
    public class CreateDealerAreaCommand : IRequest<Response<DealerAreaDTO>>
    {
        public required string AreaName { get; set; }
        public required string AreaCode { get; set; }
    }

    public class CreateDealerAreaCommandHandler : IRequestHandler<CreateDealerAreaCommand, Response<DealerAreaDTO>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerArea> _repository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _queryRepository;
        private readonly IIdentityService _identityService;

        public CreateDealerAreaCommandHandler(
            ICommandRepository<Domain.Entities.DealerArea> repository,
            IQueryRepository<Domain.Entities.DealerArea> queryRepository,
            IIdentityService identityService)
        {
            _repository = repository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<DealerAreaDTO>> Handle(CreateDealerAreaCommand request, CancellationToken cancellationToken)
        {
            // Check if AreaCode (LN ID) already exists
            var areaExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.DealerArea.AreaCode), request.AreaCode);
            if (areaExists)
            {
                return new Response<DealerAreaDTO>
                {
                    Success = false,
                    Message = $"Area code (LN ID) '{request.AreaCode}' already exists."
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

            await _repository.AddAsync(area);

            return new Response<DealerAreaDTO>
            {
                Success = true,
                Data = new DealerAreaDTO
                {
                    AreaID = area.AreaID,
                    AreaName = area.AreaName,
                    AreaCode = area.AreaCode,
                    IsActive = area.IsActive,
                    CreatedAt = area.CreatedDate,
                    UpdatedAt = area.CreatedDate
                },
                Message = "Dealer area created successfully."
            };
        }
    }

    public class UpdateDealerAreaCommand : IRequest<Response<DealerAreaDTO>>
    {
        public int AreaID { get; set; }
        public required string AreaName { get; set; }
        public required string AreaCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDealerAreaCommandHandler : IRequestHandler<UpdateDealerAreaCommand, Response<DealerAreaDTO>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerArea> _repository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _queryRepository;
        private readonly IIdentityService _identityService;

        public UpdateDealerAreaCommandHandler(
            ICommandRepository<Domain.Entities.DealerArea> repository,
            IQueryRepository<Domain.Entities.DealerArea> queryRepository,
            IIdentityService identityService)
        {
            _repository = repository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<DealerAreaDTO>> Handle(UpdateDealerAreaCommand request, CancellationToken cancellationToken)
        {
            var area = await _queryRepository.GetByIdAsync(request.AreaID);
            if (area == null)
            {
                return new Response<DealerAreaDTO>
                {
                    Success = false,
                    Message = "Area not found"
                };
            }

            area.AreaName = request.AreaName;
            area.AreaCode = request.AreaCode;
            area.IsActive = request.IsActive;
            area.ModifiedDate = DateTime.UtcNow;
            area.ModifiedBy = _identityService.GetCurrentUserId();

            await _repository.UpdateAsync(area);

            return new Response<DealerAreaDTO>
            {
                Success = true,
                Data = new DealerAreaDTO
                {
                    AreaID = area.AreaID,
                    AreaName = area.AreaName,
                    AreaCode = area.AreaCode,
                    IsActive = area.IsActive,
                    CreatedAt = area.CreatedDate,
                    UpdatedAt = area.ModifiedDate ?? area.CreatedDate
                },
                Message = "Dealer area updated successfully."
            };
        }
    }

    public class DeleteDealerAreaCommand : IRequest<Response<bool>>
    {
        public int AreaID { get; set; }
    }

    public class DeleteDealerAreaCommandHandler : IRequestHandler<DeleteDealerAreaCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerArea> _repository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _queryRepository;

        public DeleteDealerAreaCommandHandler(
            ICommandRepository<Domain.Entities.DealerArea> repository,
            IQueryRepository<Domain.Entities.DealerArea> queryRepository)
        {
            _repository = repository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<bool>> Handle(DeleteDealerAreaCommand request, CancellationToken cancellationToken)
        {
            var area = await _queryRepository.GetByIdAsync(request.AreaID);
            if (area == null)
            {
                return new Response<bool>
                {
                    Success = false,
                    Message = "Area not found"
                };
            }

            await _repository.DeleteAsync(area);

            return new Response<bool>
            {
                Success = true,
                Data = true,
                Message = "Dealer area deleted successfully."
            };
        }
    }
}
