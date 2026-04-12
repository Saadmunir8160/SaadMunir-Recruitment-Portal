using Application.DTOs;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Queries.User
{
    public class GetAllDealerUsersQuery : IRequest<List<DealerUserDTO>> { }

    public class GetAllDealerUsersQueryHandler : IRequestHandler<GetAllDealerUsersQuery, List<DealerUserDTO>>
    {
        private readonly IUserService _userService;

        public GetAllDealerUsersQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<List<DealerUserDTO>> Handle(GetAllDealerUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetAllDealerUsersAsync();
        }
    }
} 