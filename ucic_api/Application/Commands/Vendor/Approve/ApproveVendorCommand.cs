using Application.Common.Services;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Text.Json;
using Application.Common.Interfaces;

namespace Application.Commands.Vendor.Approve
{
    public static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyNullable(this JsonElement jsonElement, string propertyName)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            if (jsonElement.TryGetProperty(propertyName, out JsonElement returnElement))
            {
                if (returnElement.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }
                return returnElement;
            }
            return null;
        }
    }

    public class ApproveVendorCommand : IRequest<string>
    {
        public long Id { get; set; }
    }

    public class ApproveVendorCommandHandler : IRequestHandler<ApproveVendorCommand, string>
    {
        private readonly ICommandRepository<Domain.Entities.Vendor> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Vendor> _queryRepository;
        private readonly IExternalApiService _externalApiService;
        private readonly IEmailService _emailService;

        public ApproveVendorCommandHandler(
            ICommandRepository<Domain.Entities.Vendor> commandRepository,
            IQueryRepository<Domain.Entities.Vendor> queryRepository,
            IExternalApiService externalApiService,
            IEmailService emailService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _externalApiService = externalApiService;
            _emailService = emailService;
        }

        public async Task<string> Handle(ApproveVendorCommand request, CancellationToken cancellationToken)
        {
            var vendor = await _queryRepository.GetByIdAsync(request.Id);
            if (vendor == null)
                return null;

            try
            {
                var vendorData = new
                {
                    data = new[]
                    {
                        new
                        {
                            CompanyName = vendor.CompanyName,
                            Language = "2", // Assuming 2 is for English
                            Currency = vendor.Currency?.CurrencyCode ?? "OTH",
                            AddressName = vendor.CompanyName,
                            Name = vendor.CompanyName,
                            Street = vendor.Address,
                            Address = vendor.Address,
                            House = "",
                            PObox = "",
                            ZIP = "",
                            CityCode = vendor.CityId.ToString(),
                            Country = vendor.Country?.CountryName ?? "OTH", // Default to Saudi Arabia
                            State = "",
                            Initials = "",
                            FullName = vendor.ContactPerson,
                            FirstName = vendor.ContactPerson,
                            contactEmail= vendor.ContactPersonEmail,
                            contactMobile = vendor.ContactPersonPhoneNo,
                            crNumber = vendor.CrNo,
                            vatNumber = vendor.TaxRegistrationNo,
                        }
                    }
                };

                var response = await _externalApiService.PostAsync<dynamic>("CreateVendor", vendorData);

                // Update vendor with response data
                var jsonElement = (JsonElement)response;
                if (jsonElement.ValueKind != JsonValueKind.Null)
                {
                    // Assuming the response contains a CardCode or similar identifier
                    if (jsonElement.TryGetProperty("result", out var resultProperty) &&
                        resultProperty.ValueKind == JsonValueKind.String)
                    {
                        var resultValue = resultProperty.GetString();
                        if (!string.IsNullOrEmpty(resultValue))
                        {
                            var parts = resultValue.Split('|');
                            if (parts.Length > 1)
                            {
                                vendor.BPId = parts[1]; // This will be "LOC-00704"
                            }
                        }
                    }

                    vendor.IsApproved = true;
                    vendor.ModifiedDate = DateTime.UtcNow;
                    await _commandRepository.UpdateAsync(vendor);
                    await SendVendorApprovalEmail(vendor.CompanyName, vendor.Email, vendor.BPId!);
                }
                else
                {
                    throw new Exception("Failed to get valid response from external API");
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't fail the approval process
                Console.WriteLine($"Failed to create vendor in external system: {ex.Message}");
                throw; // Re-throw the exception to handle it at a higher level
            }
            return vendor.BPId!;
        }
        private async Task SendVendorApprovalEmail(string customerName, string customerEmail, string bpId)
        {
            var subject = "Vendor Registration Approved";
            var body = $@"  <p dir='rtl' style='text-align:right;'>عزيزي {customerName}،</p>
                            <p dir='rtl' style='text-align:right;'>يسعدنا إبلاغكم بأنه قد تم اعتماد تسجيلكم لدى شركة الأسمنت المتحدة الصناعية.</p>
                            <p dir='rtl' style='text-align:right;'>رقم تعريف الشريك التجاري الخاص بكم هو: <strong>{bpId}</strong></p>
                            <p dir='rtl' style='text-align:right;'>أنتم الآن مورد مسجل رسميًا ويمكنكم المشاركة في عمليات الشراء الخاصة بنا.</p>
                            <p dir='rtl' style='text-align:right;'>نشكركم على اهتمامكم بالتعاون مع شركة الأسمنت المتحدة الصناعية، ونتطلع إلى شراكة ناجحة ومثمرة.</p>
                            <p dir='rtl' style='text-align:right;'><strong>مع أطيب التحيات،</strong><br>قسم المشتريات<br>شركة الأسمنت المتحدة الصناعية</p>

                            <hr>
                            <p>Dear {customerName},</p>
                            <p>We are pleased to inform you that your registration with United Cement Industrial Company has been <strong>approved</strong>.</p>
                            <p>Your <strong>Business Partner ID</strong> is: <strong>{bpId}</strong></p>
                            <p>You are now officially a registered vendor and can participate in procurement processes with us.</p>
                            <p>Thank you for your interest in working with United Cement Industrial Company. We look forward to a successful partnership.</p>
                            <p><strong>Best regards,</strong><br>United Cement Industrial Company Purchase Department</p> ";


            await _emailService.SendEmailAsync(customerEmail, subject, body);
        }
    }
}
