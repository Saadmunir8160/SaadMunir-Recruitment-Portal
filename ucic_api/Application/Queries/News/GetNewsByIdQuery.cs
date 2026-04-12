using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.News
{
    public class GetNewsByIdQuery : IRequest<NewsDto>
    {
        public long NewsId { get; set; }
    }

    public class GetNewsByIdHandler : IRequestHandler<GetNewsByIdQuery, NewsDto>
    {
        private readonly IQueryRepository<Domain.Entities.News> _queryRepository;

        public GetNewsByIdHandler(IQueryRepository<Domain.Entities.News> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<NewsDto> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
        {
            var news = await _queryRepository.GetByIdAsync(request.NewsId);

            if (news == null)
                throw new KeyNotFoundException("News not found");

            return new NewsDto
            {
                Id = news.NewsId,
                Title = news.Title,
                Content = news.Content,
                ImageUrl = news.ImageUrl,
                CreatedAt = news.CreatedDate,
                IsArabic = news.IsArabic
            };
        }
    }

}
