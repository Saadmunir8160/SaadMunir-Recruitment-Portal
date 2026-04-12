using Application.Commands.ContactUs.Create;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.JobApplication.Create
{
    public class CreateJobApplicationCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "JobId is required")]
        //[StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public long JobId { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 150 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Phone { get; set; }

        public IFormFile? Resume { get; set; }
    }

    public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, Response<string>>
    {
        private readonly IQueryRepository<Domain.Entities.JobApplications> _queryRepository;
        private readonly ICommandRepository<Domain.Entities.JobApplications> _commandRepository;
        private readonly IImageFileService _FileService;
        private readonly IEmailService _emailService;

        public CreateJobApplicationCommandHandler(
            IQueryRepository<Domain.Entities.JobApplications> queryRepository,
            ICommandRepository<Domain.Entities.JobApplications> commandRepository,
            IImageFileService FileService,
            IEmailService emailService)
        {
            _queryRepository = queryRepository;
            _commandRepository = commandRepository;
            _FileService = FileService;
            _emailService = emailService;

        }
        public async Task<Response<string>> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
        {
            //First need to verify file type and size
            var allowedExtensions = new[] { ".pdf" };
            long maxFileSize = 5 * 1024 * 1024; // 5 MB in bytes

            if (request.Resume != null)
            {
                // Verify file type
                var extension = Path.GetExtension(request.Resume.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return ResponseFailure("Invalid file type. Only .pdf files are allowed.", extension);
                }

                // Verify file size
                if (request.Resume.Length > maxFileSize)
                {
                    return ResponseFailure("File size exceeds the limit of 5 MB.", request.Resume.Length.ToString());
                }
            }

            //verfy already applied

            var filters = new Dictionary<string, object>
              {
                { nameof(Domain.Entities.JobApplications.JobsId), request.JobId },
                { nameof(Domain.Entities.JobApplications.Email), request.Email }
              };
            var resultcheck = await _queryRepository.GetByColumnsAsync(filters);

            if (resultcheck.Any())
            {
                return ResponseFailure("Already Applied.", request.Email);
            }

            //Upload File on path
            string? filePath = "";
            if (request.Resume != null)
            {
                filePath = await _FileService.SaveFileAsync(request.Resume, "Resume");
            }

            // saving data in database
            var item = new Domain.Entities.JobApplications
            {
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email,
                JobsId = request.JobId,
                ResumePath = filePath,
            };

            var result = await _commandRepository.AddAsync(item);
            if (result <= 0)
            {
                if (filePath != null)
                {
                    _FileService.DeleteFile(filePath);
                }
                return ResponseFailure("Error Creating the JobApplication Request", "Not Created");
            }

            return ResponseSuccess("Applied for job Succesfully", "Success");

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
