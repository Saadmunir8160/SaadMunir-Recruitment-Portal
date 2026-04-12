using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Driver.Delete
{
    public class DeleteDriverCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteDriverCommandHandler : IRequestHandler<DeleteDriverCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Driver> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Driver> _queryRepository;

        public DeleteDriverCommandHandler(
            ICommandRepository<Domain.Entities.Driver> commandRepository,
            IQueryRepository<Domain.Entities.Driver> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<bool> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = await _queryRepository.GetByIdAsync(request.Id);
            if (driver == null)
            {
                return false;
            }

            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(driver);

            return true;
        }
    }
} 