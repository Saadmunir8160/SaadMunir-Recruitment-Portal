using Application.Commands.Jobs.Delete;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.News.Delete
{
    public class DeleteNewsCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }

    public class DeleteNewsCommandHandler : IRequestHandler<DeleteNewsCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.News> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.News> _queryRepository;

        public DeleteNewsCommandHandler(ICommandRepository<Domain.Entities.News> commandRepository, IQueryRepository<Domain.Entities.News> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }
        public async Task<bool> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
        {
            var news = await _queryRepository.GetByIdAsync(request.Id);
            if (news == null)
            {
                return false;
            }

            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(news);

            return true;
        }
    }
}
