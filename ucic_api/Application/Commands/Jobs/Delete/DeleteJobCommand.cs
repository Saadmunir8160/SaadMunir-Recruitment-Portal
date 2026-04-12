using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Jobs.Delete
{
    public class DeleteJobCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }

    public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Jobs> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Jobs> _queryRepository;

        public DeleteJobCommandHandler(ICommandRepository<Domain.Entities.Jobs> commandRepository, IQueryRepository<Domain.Entities.Jobs> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }
        public async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _queryRepository.GetByIdAsync(request.Id);
            if (job == null)
            {
                return false;
            }

            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(job);

            return true;
        }
    }
}
