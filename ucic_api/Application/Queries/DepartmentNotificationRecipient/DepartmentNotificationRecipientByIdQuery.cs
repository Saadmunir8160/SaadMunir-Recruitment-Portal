using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.DepartmentNotificationRecipient
{
    public class DepartmentNotificationRecipientByIdQuery : IRequest<DepartmentNotificationRecipientDTO>
    {
        public long Id { get; set; }
    }

    public class DepartmentNotificationRecipientByIdQueryHandler : IRequestHandler<DepartmentNotificationRecipientByIdQuery, DepartmentNotificationRecipientDTO>
    {
        private readonly IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> _queryRepository;

        public DepartmentNotificationRecipientByIdQueryHandler(IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<DepartmentNotificationRecipientDTO> Handle(DepartmentNotificationRecipientByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
           
            
            if (result == null)
                throw new KeyNotFoundException("Department Notification Recipient not found");

            return new DepartmentNotificationRecipientDTO
            {
                Id = result.Id,
                Name = result.Name,
                DepartmentId = result.DepartmentId,
                DepartmentName = result.Department.Name,
                Active = result.Active,
                EmailAddress = result.EmailAddress,
                Phone = result.PhoneNo,
                CreatedDate = result.CreatedDate
            };
        }
    }
}
