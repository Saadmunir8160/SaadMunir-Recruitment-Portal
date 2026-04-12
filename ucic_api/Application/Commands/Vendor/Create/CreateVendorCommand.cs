using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Vendor.Create
{
    public class CreateVendorCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 100 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "PhoneNo is required")]
        [StringLength(50, ErrorMessage = "PhoneNo cannot exceed 100 characters")]
        public string PhoneNo { get; set; }

        [StringLength(50, ErrorMessage = "FaxNo cannot exceed 100 characters")]
        public string? FaxNo { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "ContactPerson is required")]
        [StringLength(200, ErrorMessage = "ContactPerson cannot exceed 100 characters")]
        public string ContactPerson { get; set; }
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPhoneNo { get; set; }

        [Required(ErrorMessage = "Employees is required")]
        public int Employees { get; set; }

        [Required(ErrorMessage = "CountryId is required")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "CurrencyId is required")]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "CityId is required")]
        public long CityId { get; set; }

        [Required(ErrorMessage = "ProductDetails is required")]
        public string ProductDetails { get; set; }

        [Required(ErrorMessage = "AnnualTurnover is required")]
        public string AnnualTurnover { get; set; }

        [Required(ErrorMessage = "MajorCustomers is required")]
        public string MajorCustomers { get; set; }

        [Required(ErrorMessage = "CRNo is required")]
        [StringLength(150, ErrorMessage = "CRNo cannot exceed 50 characters")]
        public string CRNo { get; set; }

        [Required(ErrorMessage = "TaxRegistrationNo is required")]
        [StringLength(150, ErrorMessage = "TaxRegistrationNo cannot exceed 150 characters")]
        public string TaxRegistrationNo { get; set; }


        [Required(ErrorMessage = "FullName is required")]
        public bool Price { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public bool Quality { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public bool Delivery { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public bool Reference { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public bool LocationalSuitability { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public bool HSECompliance { get; set; }

        [Required(ErrorMessage = "CRCertificateFilePath is required")]
        public IFormFile? CRCertificateFilePath { get; set; }

        [Required(ErrorMessage = "VatCertificateFilePath is required")]
        public IFormFile? VatCertificateFilePath { get; set; }
        public IFormFile? ISO9001_2015FilePath { get; set; }
        public IFormFile? ISO14001_2015FilePath { get; set; }
        public IFormFile? ISO45001_2018FilePath { get; set; }
        public IFormFile? CompanyProfileFilePath { get; set; }
    }

    public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Vendor> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> _queryRepository;
        private readonly IQueryRepository<Domain.Entities.Vendor> _vendorQueryRepository;
        private readonly IImageFileService _FileService;
        private readonly IEmailService _emailService;

        public CreateVendorCommandHandler(
            ICommandRepository<Domain.Entities.Vendor> commandRepository, 
            IImageFileService fileService, 
            IEmailService emailService, 
            IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> queryRepository,
            IQueryRepository<Domain.Entities.Vendor> vendorQueryRepository)
        {
            _commandRepository = commandRepository;
            _FileService = fileService;
            _emailService = emailService;
            _queryRepository = queryRepository;
            _vendorQueryRepository = vendorQueryRepository;
        }
        public async Task<Response<string>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate TaxRegistrationNo
            var existingVendor = await _vendorQueryRepository.GetByColumnsAsync(new Dictionary<string, object>
            {
                { "TaxRegistrationNo", request.TaxRegistrationNo }
            });

            if (existingVendor.Any())
            {
                return ResponseFailure("A vendor with this Tax Registration Number already exists.", request.TaxRegistrationNo);
            }

            //First need to verify file type and size
            var allowedExtensions = new[] { ".pdf", ".png", ".jpeg", ".jpg", ".gif" };
            long maxFileSize = 5 * 1024 * 1024; // 5 MB in bytes
            string? CRCertificateFilePath = null;
            string? VatCertificateFilePath = null;
            string? ISO9001_2015FilePath = null;
            string? ISO14001_2015FilePath = null;
            string? ISO45001_2018FilePath = null;
            string? CompanyProfileFilePath = null;

            try
            {
                if (request.CRCertificateFilePath != null)
                {
                    // Verify file type
                    var extension = Path.GetExtension(request.CRCertificateFilePath.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return ResponseFailure("Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.", extension);
                    }

                    // Verify file size
                    if (request.CRCertificateFilePath.Length > maxFileSize)
                    {
                        return ResponseFailure("File size exceeds the limit of 5 MB.", request.CRCertificateFilePath.Length.ToString());
                    }

                    // Upload File to path
                    CRCertificateFilePath= await _FileService.SaveFileAsync(request.CRCertificateFilePath, "Vendors");
                }

                if (request.VatCertificateFilePath != null)
                {
                    // Verify file type
                    var extension = Path.GetExtension(request.VatCertificateFilePath.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return ResponseFailure("Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.", extension);
                    }

                    // Verify file size
                    if (request.VatCertificateFilePath.Length > maxFileSize)
                    {
                        return ResponseFailure("File size exceeds the limit of 5 MB.", request.VatCertificateFilePath.Length.ToString());
                    }

                    // Upload File to path
                    VatCertificateFilePath = await _FileService.SaveFileAsync(request.VatCertificateFilePath, "Vendors");
                }

                if (request.ISO9001_2015FilePath != null)
                {
                    // Verify file type
                    var extension = Path.GetExtension(request.ISO9001_2015FilePath.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return ResponseFailure("Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.", extension);
                    }

                    // Verify file size
                    if (request.ISO9001_2015FilePath.Length > maxFileSize)
                    {
                        return ResponseFailure("File size exceeds the limit of 5 MB.", request.ISO9001_2015FilePath.Length.ToString());
                    }

                    // Upload File to path
                    ISO9001_2015FilePath = await _FileService.SaveFileAsync(request.ISO9001_2015FilePath, "Vendors");
                }

                if (request.ISO14001_2015FilePath != null)
                {
                    // Verify file type
                    var extension = Path.GetExtension(request.ISO14001_2015FilePath.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return ResponseFailure("Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.", extension);
                    }

                    // Verify file size
                    if (request.ISO14001_2015FilePath.Length > maxFileSize)
                    {
                        return ResponseFailure("File size exceeds the limit of 5 MB.", request.ISO14001_2015FilePath.Length.ToString());
                    }

                    // Upload File to path
                    ISO14001_2015FilePath = await _FileService.SaveFileAsync(request.ISO14001_2015FilePath, "Vendors");
                }

                if (request.ISO45001_2018FilePath != null)
                {
                    // Verify file type
                    var extension = Path.GetExtension(request.ISO45001_2018FilePath.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return ResponseFailure("Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.", extension);
                    }

                    // Verify file size
                    if (request.ISO45001_2018FilePath.Length > maxFileSize)
                    {
                        return ResponseFailure("File size exceeds the limit of 5 MB.", request.ISO45001_2018FilePath.Length.ToString());
                    }

                    // Upload File to path
                    ISO45001_2018FilePath = await _FileService.SaveFileAsync(request.ISO45001_2018FilePath, "Vendors");
                }

                if (request.CompanyProfileFilePath != null)
                {
                    // Verify file type
                    var extension = Path.GetExtension(request.CompanyProfileFilePath.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return ResponseFailure("Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.", extension);
                    }

                    // Verify file size
                    if (request.CompanyProfileFilePath.Length > maxFileSize)
                    {
                        return ResponseFailure("File size exceeds the limit of 5 MB.", request.CompanyProfileFilePath.Length.ToString());
                    }

                    // Upload File to path
                    CompanyProfileFilePath = await _FileService.SaveFileAsync(request.CompanyProfileFilePath, "Vendors");
                }

                var item = new Domain.Entities.Vendor
                {
                    CompanyName = request.Name,
                    Address = request.Address,
                    PhoneNo = request.PhoneNo,
                    FaxNo = request.FaxNo,
                    Email = request.Email,
                    ContactPerson = request.ContactPerson,
                    ContactPersonEmail = request.ContactPersonEmail,
                    ContactPersonPhoneNo = request.ContactPersonPhoneNo,
                    Employees = request.Employees,
                    ProductDetails = request.ProductDetails,
                    AnnualTurnover = request.AnnualTurnover,
                    MajorCustomers = request.MajorCustomers,
                    TaxRegistrationNo = request.TaxRegistrationNo,
                    //Price = request.Price,
                    //Quality = request.Quality,
                    //Delivery = request.Delivery,
                    //Reference = request.Reference,
                    //LocationalSuitability = request.LocationalSuitability,
                    //HSECompliance = request.HSECompliance,
                    CRCertificateFilePath = CRCertificateFilePath!,
                    VatCertificateFilePath = VatCertificateFilePath!,
                    ISO9001_2015FilePath = ISO9001_2015FilePath!,
                    ISO14001_2015FilePath = ISO14001_2015FilePath!,
                    ISO45001_2018FilePath = ISO45001_2018FilePath!,
                    CompanyProfileFilePath = CompanyProfileFilePath!,
                    CountryId =request.CountryId,
                    CityId = request.CityId,
                    CategoryId = request.CategoryId,
                    CrNo = request.CRNo,
                    CurrencyId = request.CurrencyId
                };

                var result = await _commandRepository.AddAsync(item);

                if (result <= 0)
                {
                    if (CRCertificateFilePath != null)
                    {
                        _FileService.DeleteFile(CRCertificateFilePath);
                    }

                    if (VatCertificateFilePath != null)
                    {
                        _FileService.DeleteFile(VatCertificateFilePath);
                    }
                    return ResponseFailure("Error Creating the Vendor", "Not Created");
                }

                await SendEmailToVendor(request.Name, request.Email);
                await SendEmailToPurchaseDepartmentReciepents(request.Name, request.Email);

                return new Response<string>
                {
                    Success = true,
                    Message = "Vendor Created Successfully",
                    Data = "Success"
                };
            }
            catch (Exception ex)
            {
                if (CRCertificateFilePath != null)
                {
                    _FileService.DeleteFile(CRCertificateFilePath);
                }

                return ResponseFailure("An error occurred while creating the vendor.", ex.Message);
            }

        }

        private async Task SendEmailToVendor(string customerName, string CustomerEmail)
        {
            var subject = $"Vendor Registration Status Update";
            var body = $@"
                    <h2>Dear {customerName}!</h2>
                    <p>Thank you for completing your registration with United Cement Industrial Company!</p>
                    <p>At this time, your registration has been received but is still under review and has not yet been approved.</p>
                    <p>Our team is currently evaluating your submission, and we will notify you once the review process is complete.</p>
                    
                    <p>Thank you for your patience and understanding.</p>
                    <p><strong>Best regards,</strong><br>United Cement Industrial Company Purchase Department</p>";

            try
            {
                await _emailService.SendEmailAsync(CustomerEmail, subject, body);
            }
            catch (Exception ex)
            {
                // Optionally log the exception or handle as needed
                // For now, just swallow the exception to avoid breaking the flow
            }
        }

        private async Task SendEmailToPurchaseDepartmentReciepents(string customerName, string CustomerEmail)
        {
            var emailRecipients = await _queryRepository.GetAllAsync();

            var purchaseDepartmentEmails = emailRecipients
                .Where(x => x.DepartmentId == 4)
                .Select(x => x.EmailAddress)
                .ToList();

            var subject = $"New Vendor Registration Received - {customerName}";

            var body = $@"

        <div dir=""rtl"" style=""text-align: right;"">
            <p>عزيزي <strong>{customerName}</strong>،</p>
            <p>شكرًا لكم على إتمام عملية التسجيل لدى شركة الأسمنت المتحدة الصناعية!</p>
            <p>في الوقت الحالي، تم استلام تسجيلكم وهو قيد المراجعة، ولم يتم اعتماده بعد.</p>
            <p>يقوم فريقنا حاليًا بمراجعة المعلومات المقدمة، وسنقوم بإبلاغكم فور الانتهاء من عملية التقييم.</p>
            <p>نشكركم على صبركم وتفهمكم.</p>
            <p><strong>مع أطيب التحيات،</strong><br>قسم المشتريات<br>شركة الأسمنت المتحدة الصناعية</p>
        </div>

        <hr style=""margin: 30px 0; border: 1px solid #ccc;"">

        <div dir=""ltr"">
            <p>Dear <strong>{customerName}</strong>,</p>
            <p>Thank you for completing your registration with United Cement Industrial Company!</p>
            <p>At this time, your registration has been received but is still under review and has not yet been approved.</p>
            <p>Our team is currently evaluating your submission, and we will notify you once the review process is complete.</p>
            <p>Thank you for your patience and understanding.</p>
            <p><strong>Best regards,</strong><br>United Cement Industrial Company Purchase Department</p>
        </div>
    </div>";

            foreach (var email in purchaseDepartmentEmails)
            {
                await _emailService.SendEmailAsync(email, subject, body);
            }

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
