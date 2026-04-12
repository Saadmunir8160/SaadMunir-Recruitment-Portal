using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.User
{
    public class GetUserWithLocationsQuery : IRequest<UserWithLocationsDTO>
    {
        public string UserId { get; set; }
        public long coverageAreaId { get; set; }
    }

    public class GetUserWithLocationsQueryHandler : IRequestHandler<GetUserWithLocationsQuery, UserWithLocationsDTO>
    {
        private readonly IIdentityService _identityService;
        private readonly IQueryRepository<Domain.Entities.Customer> _queryRepositoryCustomer;
        private readonly IQueryRepository<Domain.Entities.Location> _queryRepositoryLocation;

        public GetUserWithLocationsQueryHandler(IIdentityService identityService, IQueryRepository<Domain.Entities.Customer> queryRepositoryCustomer, IQueryRepository<Domain.Entities.Location> queryRepositoryLocation)
        {
            _identityService = identityService;
            _queryRepositoryCustomer = queryRepositoryCustomer;
            _queryRepositoryLocation = queryRepositoryLocation;
        }
        public async Task<UserWithLocationsDTO> Handle(GetUserWithLocationsQuery request, CancellationToken cancellationToken)
        {
            var (userId, fullName, userName, email, phoneNumber, roles) = await _identityService.GetUserDetailsAsync(request.UserId);
            var filters = new Dictionary<string, object>
              {
                  { nameof(Customer.UserId), request.UserId }
              };
            var customerResult = await _queryRepositoryCustomer.GetByColumnsAsync(filters);

            var filtersLocation = new Dictionary<string, object>
              {
                { nameof(Location.CustomerId), customerResult.FirstOrDefault().CustomerId },
                { nameof(Domain.Entities.Location.CoverageAreaId), request.coverageAreaId }
              };
            var locationResult = await _queryRepositoryLocation.GetByColumnsAsync(filtersLocation);


            //return new UserWithLocationsDTO() { Id = userId, FullName = fullName, Email = email, Locations = locationResult.ToList() };
            return new UserWithLocationsDTO()
            {
                Id = userId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                CR_No = customerResult.FirstOrDefault().CR_No,
                VAT_ID = customerResult.FirstOrDefault().VAT_ID,
                ContactPerson = customerResult.FirstOrDefault().ContactPerson,
                Locations = locationResult.Select(location => new LocationDTO
                {
                    LocationID = location.LocationId,
                    CustomerId = location.CustomerId,
                    CityId = location.CitiesId,
                    Address = location.Address,
                    ZipCode = location.ZipCode,
                    GpsCoordinates = location.GpsCoordinates
                }).ToList()
            };
        }
    }
}
