using Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Common.Extensions
{
    public static class PaginationExtensions
    {
        public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
            this IQueryable<T> source,
            PaginationParameters parameters)
        {
            var totalCount = await source.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var data = await source
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginatedResponse<T>
            {
                Data = data,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                },
                Success = true,
                Message = "Data retrieved successfully."
            };
        }

        public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
            this IEnumerable<T> source,
            PaginationParameters parameters)
        {
            var totalCount = source.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            var data = source
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            return new PaginatedResponse<T>
            {
                Data = data,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                },
                Success = true,
                Message = "Data retrieved successfully."
            };
        }
    }
} 