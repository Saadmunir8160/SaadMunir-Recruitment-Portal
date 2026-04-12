using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.Dealer.Update
{
    public class UpdateDealerCommand : IRequest<DealerDTO>
    {
        public UpdateDealerDTO Dealer { get; set; }
    }

    public class UpdateDealerCommandHandler : IRequestHandler<UpdateDealerCommand, DealerDTO>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _queryRepository;
        public UpdateDealerCommandHandler(ICommandRepository<Domain.Entities.Dealer> commandRepository, IQueryRepository<Domain.Entities.Dealer> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<DealerDTO> Handle(UpdateDealerCommand request, CancellationToken cancellationToken)
        {
            var existingDealer = await _queryRepository.GetByIdAsync(request.Dealer.DealerId);
            if (existingDealer == null)
            {
                throw new Exception("Dealer not found");
            }

            existingDealer.UserId = request.Dealer.UserId;
            existingDealer.DealerName = request.Dealer.DealerName;
            existingDealer.CreditLimit = request.Dealer.CreditLimit;
            existingDealer.CurrentBalance = request.Dealer.CurrentBalance;
            existingDealer.Ln_ID = request.Dealer.Ln_ID;
            existingDealer.IsActive = request.Dealer.IsActive;

            await _commandRepository.UpdateAsync(existingDealer);

            return new DealerDTO()
            {
                DealerId = existingDealer.DealerId,
                UserId = existingDealer.UserId,
                DealerName = existingDealer.DealerName,
                CreditLimit = existingDealer.CreditLimit,
                CurrentBalance = existingDealer.CurrentBalance,
                Ln_ID = existingDealer.Ln_ID,
                IsActive = existingDealer.IsActive
            };
        }
    }
}
