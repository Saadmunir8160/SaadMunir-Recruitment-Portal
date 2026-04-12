using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.DepartmentNotificationRecipient
{
    public class DepartmentNotificationRecipientQuery : IRequest<PaginatedResponse<DepartmentNotificationRecipientDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DepartmentNotificationRecipientQueryHandler : IRequestHandler<DepartmentNotificationRecipientQuery, PaginatedResponse<DepartmentNotificationRecipientDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> _queryRepository;

        public DepartmentNotificationRecipientQueryHandler(IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<DepartmentNotificationRecipientDTO>> Handle(DepartmentNotificationRecipientQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Department Notification Recipient not found");
            var itemsQuery = query.OrderByDescending(jobs => jobs.CreatedDate).Select(item => new DepartmentNotificationRecipientDTO
            {
                 Id = item.Id,
                 Name = item.Name,
                 DepartmentId = item.DepartmentId,
                 DepartmentName = item.Department.Name,
                 Active = item.Active,
                 EmailAddress = item.EmailAddress,
                 Phone = item.PhoneNo ?? string.Empty,
                 CreatedDate = item.CreatedDate
            });
            return await itemsQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
