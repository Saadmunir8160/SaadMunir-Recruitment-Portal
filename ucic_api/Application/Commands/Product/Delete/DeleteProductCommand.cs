using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Product.Delete
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public long ProductId { get; set; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Product> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;

        public DeleteProductCommandHandler(ICommandRepository<Domain.Entities.Product> commandRepository, IQueryRepository<Domain.Entities.Product> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }
        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _queryRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return false;
            }
            
            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(product);

            return true;
        }
    }
}
