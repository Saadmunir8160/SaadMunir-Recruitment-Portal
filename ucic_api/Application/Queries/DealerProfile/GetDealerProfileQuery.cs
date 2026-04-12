using Application.DTOs.DealerProfile;
using MediatR;

namespace Application.Queries.DealerProfile
{
    public class GetDealerProfileQuery : IRequest<DealerProfileDto>
    {
    }
}