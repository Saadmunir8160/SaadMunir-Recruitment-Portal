using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Vendor.Delete
{
    public class DeleteVendorCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }

    public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Vendor> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Vendor> _queryRepository;

        public DeleteVendorCommandHandler(
            ICommandRepository<Domain.Entities.Vendor> commandRepository,
            IQueryRepository<Domain.Entities.Vendor> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<bool> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
        {
            var vendor = await _queryRepository.GetByIdAsync(request.Id);
            if (vendor == null)
                return false;

            await _commandRepository.DeleteAsync(vendor);
            return true;
        }
    }
} 