// TEMPORARILY COMMENTED OUT - Will be reimplemented with simplified dealer profile
/*
using Application.DTOs.Dealer;
using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Dealer
{
    public class UpdateDealerProfileCommand : IRequest<DealerProfileDTO>
    {
        public UpdateDealerProfileDTO Profile { get; set; }
    }

    public class UpdateDealerProfileCommandHandler : IRequestHandler<UpdateDealerProfileCommand, DealerProfileDTO>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        // Note: DealerAddress repositories removed - using DealerShippingAddress instead
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public UpdateDealerProfileCommandHandler(
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            IHttpContextAccessor httpContextAccessor,
            IMediator mediator)
        {
            _dealerCommandRepository = dealerCommandRepository;
            _dealerQueryRepository = dealerQueryRepository;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        public async Task<DealerProfileDTO> Handle(UpdateDealerProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.UserId), userId }
            };

            var dealers = await _dealerQueryRepository.GetByColumnsWithListAsync(filters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
                throw new Exception("Dealer profile not found");

            // TODO: Update dealer fields for simplified entity
            // Currently commented out until new dealer profile implementation
            // Only basic fields available: DealerName, CreditLimit, CurrentBalance, Ln_ID, DealerCode, IsVerified
            if (!string.IsNullOrEmpty(request.Profile.DealerName))
                dealer.DealerName = request.Profile.DealerName;
                
            // Note: Other profile fields will be stored in ApplicationUser or separate profile entities
            // This will be implemented in the new dealer profile functionality

            dealer.ModifiedDate = DateTime.UtcNow;
            dealer.ModifiedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            await _dealerCommandRepository.UpdateAsync(dealer);

            // Update addresses if provided
            if (request.Profile.Addresses != null)
            {
                await UpdateAddressIfProvided(dealer.DealerId, "billing", request.Profile.Addresses.Billing);
                await UpdateAddressIfProvided(dealer.DealerId, "shipping", request.Profile.Addresses.Shipping);
                await UpdateAddressIfProvided(dealer.DealerId, "registered", request.Profile.Addresses.Registered);
            }

            // Return updated profile
            return await _mediator.Send(new Queries.Dealer.GetDealerProfileQuery(), cancellationToken);
        }

        private async Task UpdateAddressIfProvided(int dealerId, string addressType, AddressDTO? addressDto)
        {
            // Note: Address updates removed - use DealerShippingAddress functionality instead
            if (addressDto == null) return;
            
            // Address updates are now handled through DealerShippingAddress entity
            // This method is kept for compatibility but does not perform actual updates
            await Task.CompletedTask;
        }
    }

    public class UploadDocumentCommand : IRequest<DocumentUploadResponseDTO>
    {
        public IFormFile File { get; set; }
        public string DocumentType { get; set; }
    }

    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentUploadResponseDTO>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _fileService;

        public UploadDocumentCommandHandler(
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            IHttpContextAccessor httpContextAccessor,
            IImageFileService fileService)
        {
            _dealerCommandRepository = dealerCommandRepository;
            _dealerQueryRepository = dealerQueryRepository;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
        }

        public async Task<DocumentUploadResponseDTO> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.UserId), userId }
            };

            var dealers = await _dealerQueryRepository.GetByColumnsWithListAsync(filters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
                throw new Exception("Dealer profile not found");

            // Validate file
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new Exception("Invalid file type. Only PDF, JPG, JPEG, and PNG files are allowed.");

            if (request.File.Length > 5 * 1024 * 1024) // 5MB
                throw new Exception("File size cannot exceed 5MB.");

            // Upload file
            var fileName = $"{request.DocumentType}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            var filePath = await _fileService.SaveFileAsync(request.File, $"Dealers/{dealer.DealerId}");

            // Update dealer document path
            switch (request.DocumentType.ToLower())
            {
                case "gstcertificate":
                    dealer.GstCertificatePath = filePath;
                    break;
                case "pancard":
                    dealer.PanCardPath = filePath;
                    break;
                case "incorporationcertificate":
                    dealer.IncorporationCertificatePath = filePath;
                    break;
                case "tradelicense":
                    dealer.TradeLicensePath = filePath;
                    break;
                case "bankstatement":
                    dealer.BankStatementPath = filePath;
                    break;
                case "cancelledcheque":
                    dealer.CancelledChequePath = filePath;
                    break;
                default:
                    throw new Exception("Invalid document type.");
            }

            dealer.ModifiedDate = DateTime.UtcNow;
            await _dealerCommandRepository.UpdateAsync(dealer);

            return new DocumentUploadResponseDTO
            {
                FileName = fileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                Message = "Document uploaded successfully"
            };
        }
    }

    public class DeleteDocumentCommand : IRequest<string>
    {
        public string DocumentType { get; set; }
    }

    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, string>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteDocumentCommandHandler(
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _dealerCommandRepository = dealerCommandRepository;
            _dealerQueryRepository = dealerQueryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.UserId), userId }
            };

            var dealers = await _dealerQueryRepository.GetByColumnsWithListAsync(filters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
                throw new Exception("Dealer profile not found");

            // Clear document path
            switch (request.DocumentType.ToLower())
            {
                case "gstcertificate":
                    dealer.GstCertificatePath = null;
                    break;
                case "pancard":
                    dealer.PanCardPath = null;
                    break;
                case "incorporationcertificate":
                    dealer.IncorporationCertificatePath = null;
                    break;
                case "tradelicense":
                    dealer.TradeLicensePath = null;
                    break;
                case "bankstatement":
                    dealer.BankStatementPath = null;
                    break;
                case "cancelledcheque":
                    dealer.CancelledChequePath = null;
                    break;
                default:
                    throw new Exception("Invalid document type.");
            }

            dealer.ModifiedDate = DateTime.UtcNow;
            await _dealerCommandRepository.UpdateAsync(dealer);

            return "Document deleted successfully";
        }
    }

    public class UpdateAddressCommand : IRequest<string>
    {
        public string AddressType { get; set; }
        public UpdateAddressDTO Address { get; set; }
    }

    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, string>
    {
        // Note: DealerAddress functionality removed - using DealerShippingAddress instead
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateAddressCommandHandler(
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _dealerQueryRepository = dealerQueryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var dealerFilters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.UserId), userId }
            };

            var dealers = await _dealerQueryRepository.GetByColumnsWithListAsync(dealerFilters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
                throw new Exception("Dealer profile not found");

            // Note: Address update functionality removed
            // Use DealerShippingAddress entity and related commands for address management
            return "Address functionality has been moved to DealerShippingAddress";
        }
    }

    public class VerifyBankDetailsCommand : IRequest<BankVerificationResponseDTO>
    {
        public VerifyBankDetailsDTO BankDetails { get; set; }
    }

    public class VerifyBankDetailsCommandHandler : IRequestHandler<VerifyBankDetailsCommand, BankVerificationResponseDTO>
    {
        public async Task<BankVerificationResponseDTO> Handle(VerifyBankDetailsCommand request, CancellationToken cancellationToken)
        {
            // Mock bank verification - in real implementation, integrate with bank API
            await Task.Delay(1000, cancellationToken); // Simulate API call

            // Simple validation
            var isValid = !string.IsNullOrEmpty(request.BankDetails.BankName) &&
                         !string.IsNullOrEmpty(request.BankDetails.AccountNumber) &&
                         !string.IsNullOrEmpty(request.BankDetails.IfscCode) &&
                         request.BankDetails.IfscCode.Length == 11;

            return new BankVerificationResponseDTO
            {
                IsValid = isValid,
                Message = isValid ? "Bank details verified successfully" : "Invalid bank details"
            };
        }
    }

    public class RequestVerificationCommand : IRequest<VerificationRequestResponseDTO>
    {
    }

    public class RequestVerificationCommandHandler : IRequestHandler<RequestVerificationCommand, VerificationRequestResponseDTO>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestVerificationCommandHandler(
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _dealerCommandRepository = dealerCommandRepository;
            _dealerQueryRepository = dealerQueryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<VerificationRequestResponseDTO> Handle(RequestVerificationCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.UserId), userId }
            };

            var dealers = await _dealerQueryRepository.GetByColumnsWithListAsync(filters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
                throw new Exception("Dealer profile not found");

            var referenceNumber = $"VRF-{DateTime.Now:yyyy}-{Random.Shared.Next(100000, 999999)}";

            dealer.VerificationReferenceNumber = referenceNumber;
            dealer.VerificationRequestedAt = DateTime.UtcNow;
            dealer.Status = "Verification Requested";
            dealer.ModifiedDate = DateTime.UtcNow;

            await _dealerCommandRepository.UpdateAsync(dealer);

            return new VerificationRequestResponseDTO
            {
                Message = "Verification request submitted successfully",
                ReferenceNumber = referenceNumber
            };
        }
    }
}
*/