using Application.Common.Configurations;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.News.Update
{
    public class UpdateNewsCommand : IRequest<Response<string>>
    {
        public long NewsId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string? Content { get; set; }
        public bool IsArabic { get; set; }

        public IFormFile? File { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UpdateNewsHandler : IRequestHandler<UpdateNewsCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.News> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.News> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _imageFileService;
        private readonly IConfiguration _configuration;

        public UpdateNewsHandler(
            ICommandRepository<Domain.Entities.News> commandRepository,
            IQueryRepository<Domain.Entities.News> queryRepository,
            IHttpContextAccessor httpContextAccessor,
        IImageFileService imageFileService,
            IConfiguration configuration)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _imageFileService = imageFileService;
            _configuration = configuration;
        }

        public async Task<Response<string>> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
        {
            var existingNews = await _queryRepository.GetByIdAsync(request.NewsId);
            if (existingNews == null)
            {
                return ResponseFailure("News not found.", "Failed");
            }

            //Verify File type
            if (request.File != null && !new[] { ".png", ".jpeg", ".jpg", ".gif" }.Contains(Path.GetExtension(request.File.FileName).ToLowerInvariant()))
            {
                return ResponseFailure("Invalid file type. Only .png, .jpeg, .jpg, and .gif are allowed.", Path.GetExtension(request.File.FileName).ToLowerInvariant().ToString());
            }

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            //Upload File on path
            string? filePath = null;
            if (request.File != null)
            {
                filePath = await _imageFileService.SaveFileAsync(request.File, "News");
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingNews.ImageUrl))
                {
                    try
                    {
                        _imageFileService.DeleteFile(existingNews.ImageUrl);
                    }
                    catch (Exception)
                    {
                        // Log the error if needed
                        // Continue with the update even if file deletion fails
                    }
                }
            }

            var fileName = "";
            existingNews.Title = request.Title;
            existingNews.Content = request.Content;
            existingNews.CreatedDate = request.CreatedDate;
            fileName = existingNews.ImageUrl;
            if (filePath != null)
            {
                var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7219";
                fileName = Path.GetFileName(filePath);
                existingNews.ImageUrl = $"{baseUrl}/api/News/{fileName}";
            }
            existingNews.ModifiedDate = DateTime.UtcNow;
            existingNews.ModifiedBy = userName;
            existingNews.ImageUrl = fileName;
            existingNews.IsArabic = request.IsArabic;

            try
            {
                await _commandRepository.UpdateAsync(existingNews);
                return ResponseSuccess("Successfully updated news.", "success");
            }
            catch (Exception)
            {
                if (filePath != null)
                {
                    try
                    {
                        _imageFileService.DeleteFile(filePath);
                    }
                    catch (Exception)
                    {
                        // Log the error if needed
                    }
                }
                return ResponseFailure("Failed to update news.", "Failed");
            }
        }

        private Response<string> ResponseSuccess(string message, string request)
        {
            return new Response<string>
            {
                Success = true,
                Message = message,
                Data = request
            };
        }

        private Response<string> ResponseFailure(string message, string request)
        {
            return new Response<string>
            {
                Success = false,
                Message = message,
                Data = request
            };
        }
    }
}