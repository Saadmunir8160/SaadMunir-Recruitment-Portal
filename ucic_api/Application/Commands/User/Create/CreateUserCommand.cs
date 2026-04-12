using Application.Commands.Product.Create;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.User.Create
{
    public class CreateUserCommand : IRequest<Response<CreateUserCommand>>
    {
        [Required(ErrorMessage = "FullName is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PhoneNo is required")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmationPassword is required")]
        public string ConfirmationPassword { get; set; }

        public string? CR_No { get; set; }
        public string? VAT_ID { get; set; }
        public string? ContactPerson { get; set; }
        public string? Location { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<CreateUserCommand>>
    {
        private readonly IIdentityService _identityService;
        private readonly ICommandRepository<Domain.Entities.Customer> _commandRepository;

        public CreateUserCommandHandler(IIdentityService identityService, ICommandRepository<Domain.Entities.Customer> commandRepository)
        {
            _identityService = identityService;
            _commandRepository = commandRepository;
        }

        public async Task<Response<CreateUserCommand>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Get the first role from the list, or use a default role if none provided
            string role = request.Roles?.FirstOrDefault() ?? "User";
            
            var result = await _identityService.CreateUserAsync(request.UserName, request.Password, request.Email, request.FullName, request.Phone, role);

            if (result.isSucceed)
            {
                var item = new Domain.Entities.Customer
                {
                    UserId = result.userId,
                    CR_No = request.CR_No,
                    VAT_ID = request.VAT_ID,
                    Location = request.Location,
                    ContactPerson = request.ContactPerson,
                    CreatedDate = DateTime.Now,
                    CreatedBy = request.UserName,
                };

                var result1 = await _commandRepository.AddAsync(item);
                if (result1 <= 0)
                {
                    await _identityService.DeleteUserAsync(result.userId);

                    return new Response<CreateUserCommand>
                    {
                        Success = false,
                        Message = result.userId,
                        Data = request
                    };
                }

                return new Response<CreateUserCommand>
                {
                    Success = true,
                    Message = result.userId,
                    Data = request
                };
            }

            return new Response<CreateUserCommand>
            {
                Success = false,
                Message = result.userId,
                Data = request
            };
        }
    }
}
