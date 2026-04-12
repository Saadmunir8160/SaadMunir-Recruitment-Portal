using Application.Commands.Vendor.Create;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.ContactUs.Create
{
    public class CreateContactUsCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }
    }

    public class CreateContactUsCommandHandler : IRequestHandler<CreateContactUsCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.ContactUsRequest> _commandRepository;

        public CreateContactUsCommandHandler(ICommandRepository<Domain.Entities.ContactUsRequest> commandRepository)
        {
            _commandRepository = commandRepository;

        }
        public async Task<Response<string>> Handle(CreateContactUsCommand request, CancellationToken cancellationToken)
        {
            var item = new Domain.Entities.ContactUsRequest
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Message = request.Message,
            };

            var result = await _commandRepository.AddAsync(item);
            if (result <= 0)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = "Error Creating the Contact Us Request",
                    Data = "Not Created"
                };
            }

            return new Response<string>
            {
                Success = true,
                Message = "Contact Us Request Created Succesfully",
                Data = "Success"
            };

        }
    }
}
