using Application.DTOs;

namespace Application.Common.Interfaces
{
    public interface IUserService
    {
        Task<List<DealerUserDTO>> GetAllDealerUsersAsync();
    }
} 