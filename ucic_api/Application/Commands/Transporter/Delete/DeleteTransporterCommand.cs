using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Transporter.Delete
{
    public class DeleteTransporterCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteTransporterCommandHandler : IRequestHandler<DeleteTransporterCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Transporter> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Transporter> _queryRepository;

        public DeleteTransporterCommandHandler(
            ICommandRepository<Domain.Entities.Transporter> commandRepository,
            IQueryRepository<Domain.Entities.Transporter> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<bool> Handle(DeleteTransporterCommand request, CancellationToken cancellationToken)
        {
            var transporter = await _queryRepository.GetByIdAsync(request.Id);
            if (transporter == null)
            {
                return false;
            }

            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(transporter);

            return true;
        }
    }
} 