using Application.Common.Extensions;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.News
{
    public class GetNewsQuery : IRequest<PaginatedResponse<NewsDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetNewsHandler : IRequestHandler<GetNewsQuery, PaginatedResponse<NewsDto>>
    {
        private readonly IQueryRepository<Domain.Entities.News> _queryRepository;

        public GetNewsHandler(IQueryRepository<Domain.Entities.News> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<NewsDto>> Handle(GetNewsQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("News not found");

            var newsQuery = query
                .OrderByDescending(news => news.NewsId)
                .Select(news => new NewsDto
                {
                    Id = news.NewsId,
                    Title = news.Title,
                    Content = news.Content,
                    ImageUrl = news.ImageUrl,
                    CreatedAt = news.CreatedDate,
                    IsArabic = news.IsArabic
                });

            return await newsQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
