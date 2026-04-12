using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Application.DTOs;

namespace Application.Commands.Promotion.Delete
{
    public class DeletePromotionCommand : IRequest<bool>
    {
        [Required(ErrorMessage = "PromotionId is required")]
        public long PromotionId { get; set; }
    }

    public class DeletePromotionHandler : IRequestHandler<DeletePromotionCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Promotion> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Promotion> _queryRepository;

        public DeletePromotionHandler(
            ICommandRepository<Domain.Entities.Promotion> commandRepository,
            IQueryRepository<Domain.Entities.Promotion> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<bool> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await _queryRepository.GetByIdAsync(request.PromotionId);
            if (promotion == null)
            {
                return false;
            }

            await _commandRepository.DeleteAsync(promotion);
            return true;
        }
    }
} 