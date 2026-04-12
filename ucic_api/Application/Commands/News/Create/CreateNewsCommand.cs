using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.News.Create
{
    public class CreateNewsCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Content is required")]
        public string? Content { get; set; }
        public bool IsArabic { get; set; }

        public IFormFile? File { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateNewsHandler : IRequestHandler<CreateNewsCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.News> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _imageFileService;

        public CreateNewsHandler(
            ICommandRepository<Domain.Entities.News> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            IImageFileService imageFileService)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _imageFileService = imageFileService;
        }

        public async Task<Response<string>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
        {
            //Verify File type
            if (request.File != null && !new[] { ".png", ".jpeg", ".jpg", ".gif" }.Contains(Path.GetExtension(request.File.FileName).ToLowerInvariant()))
            {
                return ResponseFailure("Invalid file type. Only .png, .jpeg, .jpg, and .gif are allowed.", Path.GetExtension(request.File.FileName).ToLowerInvariant().ToString());
            }
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            //Upload File on path
            string? filePath = null;
            string fileName = "";
            if (request.File != null)
            {
                //filePath = await SaveFileAsync(request.File);
                filePath = await _imageFileService.SaveFileAsync(request.File, "News");
                fileName = Path.GetFileName(filePath);
            }

            var news = new Domain.Entities.News
            {
                Title = request.Title,
                Content = request.Content,
                ImageUrl = fileName,
                CreatedDate = request.CreatedDate,
                IsArabic = request.IsArabic
            };
            var resultPayment = await _commandRepository.AddAsync(news);

            if (resultPayment <= 0)
            {
                if (filePath != null)
                {
                    _imageFileService.DeleteFile(filePath);
                }
                return ResponseFailure("Failed to create or payment.", "Failed");
            }
            return ResponseSuccess("Succesfully created news.", "success"); ;
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
