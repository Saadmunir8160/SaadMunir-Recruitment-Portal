using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class DeleteDealerDriverCommand : IRequest<Response<bool>>
    {
        public int DriverID { get; set; }
    }

    public class DeleteDealerDriverCommandHandler : IRequestHandler<DeleteDealerDriverCommand, Response<bool>>
    {
        private readonly ICommandRepository<DealerDriver> _commandRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<DealerDriver> _queryRepository;
        public DeleteDealerDriverCommandHandler(ICommandRepository<DealerDriver> commandRepository, Domain.Repositories.Query.Base.IQueryRepository<DealerDriver> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }
        public async Task<Response<bool>> Handle(DeleteDealerDriverCommand request, CancellationToken cancellationToken)
        {
            var entity = await _queryRepository.GetByIdAsync(request.DriverID);
            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer driver not found." };
            
            // Hard delete - will fail if driver has orders due to RESTRICT constraint
            try
            {
                await _commandRepository.HardDeleteAsync(entity);
                return new Response<bool> { Success = true, Data = true, Message = "Dealer driver deleted successfully." };
            }
            catch (Exception ex)
            {
                return new Response<bool> 
                { 
                    Success = false, 
                    Message = "Cannot delete driver with existing orders or logs. Please remove associations first." 
                };
            }
        }
    }
}
