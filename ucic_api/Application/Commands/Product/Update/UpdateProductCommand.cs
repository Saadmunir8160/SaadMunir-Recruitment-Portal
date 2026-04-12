using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Product.Update
{
    public class UpdateProductCommand : IRequest<Response<string>>
    {
        public long ProductId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "CoverageAreaId is required")]
        public long CoverageAreaId { get; set; }
        public IFormFile? ProductFilePath { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "VatPercentage is required")]
        public decimal VatPercentage { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(30, ErrorMessage = "Sku cannot exceed 100 characters")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "ProductCode is required")]
        [StringLength(100, ErrorMessage = "ProductCode cannot exceed 100 characters")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(30, ErrorMessage = "Type cannot exceed 100 characters")]
        public string Type { get; set; }

        [Required(ErrorMessage = "DiscountPercentage is required")]
        public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "ShipingCostPercentage is required")]
        public decimal ShipingCostPercentage { get; set; }
        public string? ArabicName { get; set; }
        public string? ArabicDescription { get; set; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Product> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _imageFileService;
        private readonly IConfiguration _configuration;

        public UpdateProductCommandHandler(
            ICommandRepository<Domain.Entities.Product> commandRepository,
            IQueryRepository<Domain.Entities.Product> queryRepository,
            IHttpContextAccessor httpContextAccessor,
            IImageFileService imageFileService,
            IConfiguration configuration)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _imageFileService = imageFileService;
            _configuration = configuration;
        }

        public async Task<Response<string>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _queryRepository.GetByIdAsync(request.ProductId);
            if (existingProduct == null)
            {
                return ResponseFailure("Product not found.", "Failed");
            }

            //Verify File type
            if (request.ProductFilePath != null && !new[] { ".png", ".jpeg", ".jpg", ".gif" }.Contains(Path.GetExtension(request.ProductFilePath.FileName).ToLowerInvariant()))
            {
                return ResponseFailure("Invalid file type. Only .png, .jpeg, .jpg, and .gif are allowed.", Path.GetExtension(request.ProductFilePath.FileName).ToLowerInvariant().ToString());
            }

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            //Upload File on path
            string? filePath = null;
            if (request.ProductFilePath != null)
            {
                filePath = await _imageFileService.SaveFileAsync(request.ProductFilePath, "Product");
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    try
                    {
                        _imageFileService.DeleteFile(existingProduct.ImageUrl);
                    }
                    catch (Exception)
                    {
                        // Log the error if needed
                        // Continue with the update even if file deletion fails
                    }
                }
            }
            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.VatPercentage = request.VatPercentage;
            existingProduct.CoverageAreaId = request.CoverageAreaId;
            existingProduct.Sku =request.Sku;
            existingProduct.Code = request.ProductCode;
            existingProduct.Type = request.Type;
            existingProduct.DiscountPercentage = request.DiscountPercentage;
            existingProduct.ShippingCostPercentage = request.ShipingCostPercentage;
            existingProduct.ArabicName = request.ArabicName;
            existingProduct.ArabicDescription = request.ArabicDescription;
            var fileName = existingProduct.ImageUrl;
            if (filePath != null)
            {
                var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7219";
                fileName = Path.GetFileName(filePath);
                existingProduct.ImageUrl = $"{baseUrl}/api/Product/{fileName}";
            }
            existingProduct.ModifiedDate = DateTime.UtcNow;
            existingProduct.ModifiedBy = userName;
            existingProduct.ImageUrl = fileName;

            try
            {
                await _commandRepository.UpdateAsync(existingProduct);
                return ResponseSuccess("Successfully updated product.", "success");
            }
            catch (Exception)
            {
                if (filePath != null)
                {
                    try
                    {
                        _imageFileService.DeleteFile(filePath);
                    }
                    catch (Exception)
                    {
                        // Log the error if needed
                    }
                }
                return ResponseFailure("Failed to update product.", "Failed");
            }
        }

        private Response<string> ResponseSuccess(string message, string request)
        {
            return new Response<string>
            {
                Success = true,
                Message = message,
                Data = request
            };
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