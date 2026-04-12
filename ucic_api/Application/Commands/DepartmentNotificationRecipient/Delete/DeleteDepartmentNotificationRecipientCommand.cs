using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.DepartmentNotificationRecipient.Delete
{
    public class DeleteDepartmentNotificationRecipientCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }

    public class DeleteDepartmentNotificationRecipientCommandHandler : IRequestHandler<DeleteDepartmentNotificationRecipientCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.DepartmentNotificationRecipient> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> _queryRepository;

        public DeleteDepartmentNotificationRecipientCommandHandler(ICommandRepository<Domain.Entities.DepartmentNotificationRecipient> commandRepository, IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }
        public async Task<bool> Handle(DeleteDepartmentNotificationRecipientCommand request, CancellationToken cancellationToken)
        {
            var departmentNotificationRecipient = await _queryRepository.GetByIdAsync(request.Id);
            if (departmentNotificationRecipient == null)
            {
                return false;
            }

            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(departmentNotificationRecipient);

            return true;
        }
    }
}
