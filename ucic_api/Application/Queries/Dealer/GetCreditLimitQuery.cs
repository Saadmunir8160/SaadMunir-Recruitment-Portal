using Application.Common.Interfaces;
using Application.Common.Services;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Text.Json;

namespace Application.Queries.Dealer
{
    public class GetCreditLimitQuery : IRequest<Response<CreditLimitResponseDTO>>
    {
    }

    public class CreditLimitResponseDTO
    {
        public decimal AvailableCredit { get; set; }
    }

    public class GetCreditLimitQueryHandler : IRequestHandler<GetCreditLimitQuery, Response<CreditLimitResponseDTO>>
    {
        private readonly IIdentityService _identityService;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IExternalApiService _externalApiService;

        public GetCreditLimitQueryHandler(
            IIdentityService identityService,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IExternalApiService externalApiService)
        {
            _identityService = identityService;
            _dealerRepository = dealerRepository;
            _externalApiService = externalApiService;
        }

        public async Task<Response<CreditLimitResponseDTO>> Handle(GetCreditLimitQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get current dealer ID
                var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
                if (currentDealerId == null)
                {
                    return new Response<CreditLimitResponseDTO>
                    {
                        Success = false,
                        Message = "Dealer not found for current user"
                    };
                }

                // Get dealer to retrieve Ln_ID (Business Partner ID)
                var dealer = await _dealerRepository.GetByIdAsync(currentDealerId.Value);
                if (dealer == null || string.IsNullOrEmpty(dealer.Ln_ID))
                {
                    return new Response<CreditLimitResponseDTO>
                    {
                        Success = false,
                        Message = "Dealer Ln_ID not found. Please ensure your dealer profile is properly configured."
                    };
                }

                // Prepare query parameters for the API call
                var queryParameters = new Dictionary<string, string>
                {
                    { "Check_Business_Partner", dealer.Ln_ID }
                };

                // Call external API to get credit limit
                var apiResponse = await _externalApiService.GetAsync<dynamic>("GetCreditLimit", queryParameters);

                // Parse the JSON response to extract Available_Credit
                // Response structure: {"data":[{"Business_Partner":"1087","Credit_Limit":"10000","Available_Credit":"9000"}]}
                var jsonElement = (JsonElement)apiResponse;
                decimal availableCredit = 0;

                if (jsonElement.ValueKind != JsonValueKind.Null)
                {
                    // First, get the "data" property which is an array
                    if (jsonElement.TryGetProperty("data", out var dataProperty) && 
                        dataProperty.ValueKind == JsonValueKind.Array)
                    {
                        // Get the first element from the array
                        if (dataProperty.GetArrayLength() > 0)
                        {
                            var firstItem = dataProperty[0];
                            
                            // Now get Available_Credit from the first item
                            if (firstItem.TryGetProperty("Available_Credit", out var availableCreditProperty))
                            {
                                if (availableCreditProperty.ValueKind == JsonValueKind.Number)
                                {
                                    availableCredit = availableCreditProperty.GetDecimal();
                                }
                                else if (availableCreditProperty.ValueKind == JsonValueKind.String)
                                {
                                    // Handle string representation of number
                                    var stringValue = availableCreditProperty.GetString();
                                    if (!string.IsNullOrEmpty(stringValue) && decimal.TryParse(stringValue, out var parsedValue))
                                    {
                                        availableCredit = parsedValue;
                                    }
                                }
                            }
                        }
                    }
                }

                return new Response<CreditLimitResponseDTO>
                {
                    Success = true,
                    Message = "Credit limit retrieved successfully",
                    Data = new CreditLimitResponseDTO
                    {
                        AvailableCredit = availableCredit
                    }
                };
            }
            catch (Exception ex)
            {
                return new Response<CreditLimitResponseDTO>
                {
                    Success = false,
                    Message = $"Error retrieving credit limit: {ex.Message}"
                };
            }
        }
    }
}

