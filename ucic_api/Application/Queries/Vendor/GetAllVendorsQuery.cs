using System.ComponentModel.DataAnnotations;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Vendor
{
    public class GetAllVendorsQuery : IRequest<PaginatedResponse<VendorDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllVendorsQueryHandler : IRequestHandler<GetAllVendorsQuery, PaginatedResponse<VendorDto>>
    {
        private readonly IQueryRepository<Domain.Entities.Vendor> _queryRepository;

        public GetAllVendorsQueryHandler(IQueryRepository<Domain.Entities.Vendor> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<VendorDto>> Handle(GetAllVendorsQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Vendors not found");
            var vendorsQuery = query.Select(vendor => new VendorDto
            {
                VendorId = vendor.VendorId,
                Name = vendor.CompanyName,
                Address = vendor.Address,
                PhoneNo = vendor.PhoneNo,
                FaxNo = vendor.FaxNo!,
                Email = vendor.Email,
                ContactPerson = vendor.ContactPerson,
                ContactPersonPhoneNo = vendor.ContactPersonPhoneNo,
                ContactPersonEmail = vendor.ContactPersonEmail,
                CrNo = vendor.CrNo,
                BPId = vendor.BPId,
                Employees = vendor.Employees,
                ProductDetails = vendor.ProductDetails,
                AnnualTurnover = vendor.AnnualTurnover,
                MajorCustomers = vendor.MajorCustomers,
                TaxRegistrationNo = vendor.TaxRegistrationNo,
                Price = vendor.Price,
                Quality = vendor.Quality,
                IsApproved = vendor.IsApproved,
                CRCertificateFilePath = vendor.CRCertificateFilePath,
                VatCertificateFilePath = vendor.VatCertificateFilePath,
                ISO9001_2015FilePath = vendor.ISO9001_2015FilePath,
                ISO14001_2015FilePath = vendor.ISO14001_2015FilePath,
                ISO45001_2018FilePath = vendor.ISO45001_2018FilePath,
                CompanyProfileFilePath = vendor.CompanyProfileFilePath,
                Category = vendor.Category.CategoryCode,
            });
            return await vendorsQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
